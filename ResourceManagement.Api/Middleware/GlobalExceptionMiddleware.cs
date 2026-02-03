using System;
using System.Net;
using System.Collections.Generic;
using System.Text.Json;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ResourceManagement.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new 
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred.",
                Detail = exception.Message // In production, we might want to hide details
            };

            // Map specific exceptions to status codes
            if (exception is KeyNotFoundException) response = new { StatusCode = (int)HttpStatusCode.NotFound, Message = exception.Message, Detail = "Resource not found" };
            else if (exception is UnauthorizedAccessException) response = new { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Unauthorized", Detail = "Access denied" };
            else if (exception is ArgumentException) response = new { StatusCode = (int)HttpStatusCode.BadRequest, Message = exception.Message, Detail = "Invalid argument" };

            context.Response.StatusCode = response.StatusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
