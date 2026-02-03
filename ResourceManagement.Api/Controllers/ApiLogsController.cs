using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Api.Controllers
{
    /// <summary>
    /// Controller for querying API request logs for debugging and audit purposes.
    /// </summary>
    [Authorize(AuthenticationSchemes = "Basic")]
    [ApiController]
    [Route("api/[controller]")]
    public class ApiLogsController : ControllerBase
    {
        private readonly IApiRequestLogRepository _logRepository;

        public ApiLogsController(IApiRequestLogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        /// <summary>
        /// Gets the most recent API request logs.
        /// </summary>
        /// <param name="count">Number of logs to retrieve (default: 20, max: 100)</param>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(int count = 20)
        {
            // Limit to max 100 entries per request
            count = Math.Min(Math.Max(1, count), 100);
            
            var logs = await _logRepository.GetRecentAsync(count);
            return Ok(logs);
        }

        /// <summary>
        /// Gets a specific API request log by its correlation ID.
        /// </summary>
        /// <param name="correlationId">The unique correlation ID of the request</param>
        [HttpGet("{correlationId:guid}")]
        public async Task<IActionResult> GetByCorrelationId(Guid correlationId)
        {
            var log = await _logRepository.GetByCorrelationIdAsync(correlationId);
            
            if (log == null)
            {
                return NotFound(new { Message = $"Log with correlation ID {correlationId} not found" });
            }
            
            return Ok(log);
        }

        /// <summary>
        /// Searches API request logs by various criteria.
        /// </summary>
        /// <param name="startDate">Filter logs from this date (inclusive)</param>
        /// <param name="endDate">Filter logs until this date (inclusive)</param>
        /// <param name="username">Filter by username</param>
        /// <param name="statusCode">Filter by HTTP status code</param>
        /// <param name="path">Filter by request path (partial match)</param>
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? username = null,
            int? statusCode = null,
            string? path = null)
        {
            var logs = await _logRepository.SearchAsync(startDate, endDate, username, statusCode, path);
            return Ok(logs);
        }

        /// <summary>
        /// Gets API request logs that resulted in errors (4xx or 5xx status codes).
        /// </summary>
        /// <param name="count">Number of logs to retrieve (default: 20)</param>
        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors(int count = 20)
        {
            count = Math.Min(Math.Max(1, count), 100);
            
            // Search for logs with status codes >= 400
            var logs = await _logRepository.SearchAsync(
                startDate: null,
                endDate: null,
                username: null,
                statusCode: null,
                path: null);
            
            // Filter for error status codes and take the requested count
            var errorLogs = new System.Collections.Generic.List<Domain.Entities.ApiRequestLog>();
            foreach (var log in logs)
            {
                if (log.ResponseStatusCode >= 400)
                {
                    errorLogs.Add(log);
                    if (errorLogs.Count >= count)
                        break;
                }
            }
            
            return Ok(errorLogs);
        }
    }
}
