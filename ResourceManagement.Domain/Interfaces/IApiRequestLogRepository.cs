using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for API request logging operations.
    /// </summary>
    public interface IApiRequestLogRepository
    {
        /// <summary>
        /// Creates a new API request log entry and returns its ID.
        /// </summary>
        Task<int> LogAsync(ApiRequestLog log);
        
        /// <summary>
        /// Updates an existing log entry with response details.
        /// </summary>
        Task UpdateResponseAsync(int id, int statusCode, string? responseBody, 
                                  DateTime responseTimestamp, int durationMs, 
                                  string? exceptionMessage, string? exceptionStack);
        
        /// <summary>
        /// Gets the most recent API request logs.
        /// </summary>
        Task<IEnumerable<ApiRequestLog>> GetRecentAsync(int count);
        
        /// <summary>
        /// Searches API request logs by various criteria.
        /// </summary>
        Task<IEnumerable<ApiRequestLog>> SearchAsync(
            DateTime? startDate, 
            DateTime? endDate, 
            string? username, 
            int? statusCode, 
            string? path);
        
        /// <summary>
        /// Gets a specific API request log by its correlation ID.
        /// </summary>
        Task<ApiRequestLog?> GetByCorrelationIdAsync(Guid correlationId);
    }
}
