using System;

namespace ResourceManagement.Domain.Entities
{
    /// <summary>
    /// Entity for logging API requests and responses for audit/debugging purposes.
    /// </summary>
    public class ApiRequestLog
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Unique correlation ID for tracing the request end-to-end.
        /// </summary>
        public Guid CorrelationId { get; set; }
        
        /// <summary>
        /// When the request was received by the server.
        /// </summary>
        public DateTime RequestTimestamp { get; set; }
        
        /// <summary>
        /// When the response was sent back to the client.
        /// </summary>
        public DateTime? ResponseTimestamp { get; set; }
        
        /// <summary>
        /// Total request processing time in milliseconds.
        /// </summary>
        public int? DurationMs { get; set; }
        
        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// Request path (e.g., /api/projects/123)
        /// </summary>
        public string RequestPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Query string parameters.
        /// </summary>
        public string? QueryString { get; set; }
        
        /// <summary>
        /// Request body (JSON payload).
        /// </summary>
        public string? RequestBody { get; set; }
        
        /// <summary>
        /// HTTP status code of the response.
        /// </summary>
        public int? ResponseStatusCode { get; set; }
        
        /// <summary>
        /// Response body (truncated if too large).
        /// </summary>
        public string? ResponseBody { get; set; }
        
        /// <summary>
        /// Authenticated user's ID (RosterId).
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// Authenticated user's username.
        /// </summary>
        public string? Username { get; set; }
        
        /// <summary>
        /// Authenticated user's role (Admin, Manager, Employee).
        /// </summary>
        public string? UserRole { get; set; }
        
        /// <summary>
        /// Client's User-Agent header.
        /// </summary>
        public string? UserAgent { get; set; }
        
        /// <summary>
        /// Client's IP address.
        /// </summary>
        public string? IpAddress { get; set; }
        
        /// <summary>
        /// Exception message if the request failed.
        /// </summary>
        public string? ExceptionMessage { get; set; }
        
        /// <summary>
        /// Stack trace if the request failed.
        /// </summary>
        public string? ExceptionStack { get; set; }
    }
}
