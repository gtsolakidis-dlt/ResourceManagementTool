using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using Serilog.Context;

namespace ResourceManagement.Api.Middleware
{
    /// <summary>
    /// Middleware that logs all HTTP requests and responses to the database and structured logs.
    /// </summary>
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;
        
        // Maximum size for request/response bodies to prevent excessive storage
        private const int MaxBodyLength = 32768; // 32KB

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IApiRequestLogRepository logRepository)
        {
            // Skip logging for health checks, swagger, and static files
            if (ShouldSkipLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var correlationId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();
            
            // Add correlation ID to response headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Correlation-ID"] = correlationId.ToString();
                return Task.CompletedTask;
            });
            
            // Capture request details early
            var requestBody = await CaptureRequestBodyAsync(context.Request);
            var (userId, username, userRole) = ExtractUserInfo(context.User);
            var endpointName = context.GetEndpoint()?.DisplayName;
            var safeHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => RedactHeader(h.Key, h.Value.ToString()));

            // Push properties to Serilog Context
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestBody", requestBody))
            using (LogContext.PushProperty("RequestHeaders", safeHeaders, destructureObjects: true))
            using (LogContext.PushProperty("Endpoint", endpointName))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Username", username))
            {
                // Create log entry for DB
                var logEntry = new ApiRequestLog
                {
                    CorrelationId = correlationId,
                    RequestTimestamp = DateTime.UtcNow,
                    HttpMethod = context.Request.Method,
                    RequestPath = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    RequestBody = TruncateBody(requestBody),
                    UserId = userId,
                    Username = username,
                    UserRole = userRole,
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    IpAddress = GetClientIpAddress(context)
                };

                _logger.LogInformation(
                    "Request started: {Method} {Path} by {Username}",
                    logEntry.HttpMethod, logEntry.RequestPath, username ?? "anonymous");

                string? responseBody = null;
                string? exceptionMessage = null;
                string? exceptionStack = null;
                
                // Wrap the response body stream to capture the response
                var originalBodyStream = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;
                
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    exceptionMessage = ex.Message;
                    exceptionStack = ex.StackTrace;
                    
                    _logger.LogError(ex, 
                        "Request failed: {Method} {Path} - {Error}",
                        logEntry.HttpMethod, logEntry.RequestPath, ex.Message);
                    
                    throw; // Re-throw to let GlobalExceptionMiddleware handle it
                }
                finally
                {
                    stopwatch.Stop();
                    // Capture response body if it's JSON content
                    try
                    {
                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        
                        // Only capture JSON responses
                        var contentType = context.Response.ContentType ?? "";
                        if (contentType.Contains("application/json") || contentType.Contains("text/"))
                        {
                            using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, leaveOpen: true);
                            responseBody = await reader.ReadToEndAsync();
                        }
                        
                        // Copy response back to original stream
                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        await responseBodyStream.CopyToAsync(originalBodyStream);
                    }
                    catch (Exception copyEx)
                    {
                        _logger.LogWarning(copyEx, "Failed to capture response body");
                    }
                    finally
                    {
                        context.Response.Body = originalBodyStream;
                    }
                    
                    // Update log entry with response details
                    logEntry.ResponseStatusCode = context.Response.StatusCode;
                    logEntry.ResponseBody = TruncateBody(responseBody);
                    logEntry.ResponseTimestamp = DateTime.UtcNow;
                    logEntry.DurationMs = (int)stopwatch.ElapsedMilliseconds;
                    logEntry.ExceptionMessage = exceptionMessage;
                    logEntry.ExceptionStack = exceptionStack;
                    
                    // Log to database asynchronously
                    _ = LogToDatabaseAsync(logRepository, logEntry);
                    
                    _logger.LogInformation(
                        "Request completed: {Method} {Path} - {StatusCode} in {DurationMs}ms. Response: {ResponseBody}",
                        logEntry.HttpMethod, logEntry.RequestPath, 
                        context.Response.StatusCode, stopwatch.ElapsedMilliseconds,
                        logEntry.ResponseBody);
                }
            }
        }

        private async Task LogToDatabaseAsync(IApiRequestLogRepository logRepository, ApiRequestLog logEntry)
        {
            try
            {
                await logRepository.LogAsync(logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log API request to database");
            }
        }

        private static bool ShouldSkipLogging(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";
            return pathValue.StartsWith("/health") ||
                   pathValue.StartsWith("/swagger") ||
                   pathValue.StartsWith("/favicon");
        }

        private static async Task<string?> CaptureRequestBodyAsync(HttpRequest request)
        {
            // Only capture body for methods that typically have one
            if (request.Method == "GET" || request.Method == "DELETE" || request.Method == "HEAD" || request.Method == "OPTIONS")
            {
                return null;
            }
            
            // Skip if no content
            if (request.ContentLength == null || request.ContentLength == 0)
            {
                return null;
            }
            
            try
            {
                request.EnableBuffering();
                request.Body.Position = 0;
                
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                
                return string.IsNullOrWhiteSpace(body) ? null : body;
            }
            catch
            {
                // If we can't read the body, just skip it
                return null;
            }
        }

        private static (int? userId, string? username, string? userRole) ExtractUserInfo(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return (null, null, null);
            }

            var userIdStr = user.FindFirst("id")?.Value ?? 
                           user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            int.TryParse(userIdStr, out var userId);
            
            var username = user.FindFirst("username")?.Value ?? 
                          user.FindFirst(ClaimTypes.Name)?.Value;
            
            var userRole = user.FindFirst("role")?.Value ?? 
                          user.FindFirst(ClaimTypes.Role)?.Value;

            return (userId > 0 ? userId : null, username, userRole);
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers (when behind a proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
            
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static string? TruncateBody(string? body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return body;
            }
            
            if (body.Length <= MaxBodyLength)
            {
                return body;
            }
            
            return body.Substring(0, MaxBodyLength) + "... [truncated]";
        }

        private static string RedactHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return value;
            var upperKey = key.ToUpperInvariant();
            if (upperKey == "AUTHORIZATION" || 
                upperKey == "COOKIE" || 
                upperKey == "SET-COOKIE" || 
                upperKey == "X-API-KEY")
                return "[REDACTED]";
            return value;
        }
    }
}
