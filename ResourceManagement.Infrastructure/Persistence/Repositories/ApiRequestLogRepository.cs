using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository implementation for API request logging using Dapper.
    /// </summary>
    public class ApiRequestLogRepository : IApiRequestLogRepository
    {
        private readonly DapperContext _context;

        public ApiRequestLogRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> LogAsync(ApiRequestLog log)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO ApiRequestLogs 
                    (CorrelationId, RequestTimestamp, ResponseTimestamp, DurationMs,
                     HttpMethod, RequestPath, QueryString, RequestBody, ResponseStatusCode, ResponseBody,
                     UserId, Username, UserRole, UserAgent, IpAddress, ExceptionMessage, ExceptionStack)
                VALUES 
                    (@CorrelationId, @RequestTimestamp, @ResponseTimestamp, @DurationMs,
                     @HttpMethod, @RequestPath, @QueryString, @RequestBody, @ResponseStatusCode, @ResponseBody,
                     @UserId, @Username, @UserRole, @UserAgent, @IpAddress, @ExceptionMessage, @ExceptionStack);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                log.CorrelationId,
                log.RequestTimestamp,
                log.ResponseTimestamp,
                log.DurationMs,
                log.HttpMethod,
                log.RequestPath,
                log.QueryString,
                log.RequestBody,
                log.ResponseStatusCode,
                log.ResponseBody,
                log.UserId,
                log.Username,
                log.UserRole,
                log.UserAgent,
                log.IpAddress,
                log.ExceptionMessage,
                log.ExceptionStack
            });
        }

        public async Task UpdateResponseAsync(int id, int statusCode, string? responseBody,
                                               DateTime responseTimestamp, int durationMs,
                                               string? exceptionMessage, string? exceptionStack)
        {
            var connection = _context.GetOpenConnection();
            const string sql = @"
                UPDATE ApiRequestLogs 
                SET ResponseStatusCode = @StatusCode,
                    ResponseBody = @ResponseBody,
                    ResponseTimestamp = @ResponseTimestamp,
                    DurationMs = @DurationMs,
                    ExceptionMessage = @ExceptionMessage,
                    ExceptionStack = @ExceptionStack
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                StatusCode = statusCode,
                ResponseBody = responseBody,
                ResponseTimestamp = responseTimestamp,
                DurationMs = durationMs,
                ExceptionMessage = exceptionMessage,
                ExceptionStack = exceptionStack
            });
        }

        public async Task<IEnumerable<ApiRequestLog>> GetRecentAsync(int count)
        {
            var connection = _context.GetOpenConnection();
            const string sql = @"
                SELECT TOP (@Count) * 
                FROM ApiRequestLogs 
                ORDER BY RequestTimestamp DESC";
            
            return await connection.QueryAsync<ApiRequestLog>(sql, new { Count = count });
        }

        public async Task<IEnumerable<ApiRequestLog>> SearchAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? username,
            int? statusCode,
            string? path)
        {
            var connection = _context.GetOpenConnection();
            var sql = new StringBuilder(@"
                SELECT TOP 100 * 
                FROM ApiRequestLogs 
                WHERE 1=1");
            
            var parameters = new DynamicParameters();
            
            if (startDate.HasValue)
            {
                sql.Append(" AND RequestTimestamp >= @StartDate");
                parameters.Add("StartDate", startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                sql.Append(" AND RequestTimestamp <= @EndDate");
                parameters.Add("EndDate", endDate.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(username))
            {
                sql.Append(" AND Username = @Username");
                parameters.Add("Username", username);
            }
            
            if (statusCode.HasValue)
            {
                sql.Append(" AND ResponseStatusCode = @StatusCode");
                parameters.Add("StatusCode", statusCode.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(path))
            {
                sql.Append(" AND RequestPath LIKE @Path");
                parameters.Add("Path", $"%{path}%");
            }
            
            sql.Append(" ORDER BY RequestTimestamp DESC");
            
            return await connection.QueryAsync<ApiRequestLog>(sql.ToString(), parameters);
        }

        public async Task<ApiRequestLog?> GetByCorrelationIdAsync(Guid correlationId)
        {
            var connection = _context.GetOpenConnection();
            const string sql = @"
                SELECT TOP 1 * 
                FROM ApiRequestLogs 
                WHERE CorrelationId = @CorrelationId";
            
            return await connection.QueryFirstOrDefaultAsync<ApiRequestLog>(sql, new { CorrelationId = correlationId });
        }
    }
}
