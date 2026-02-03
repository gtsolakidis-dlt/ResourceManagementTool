using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Infrastructure.Persistence;

namespace ResourceManagement.Infrastructure.Persistence.Repositories
{
    public class ForecastRepository : IForecastRepository
    {
        private readonly DapperContext _context;

        public ForecastRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<ForecastVersion?> GetVersionByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM ForecastVersion WHERE Id = @Id";
            return await connection.QuerySingleOrDefaultAsync<ForecastVersion>(sql, new { Id = id });
        }

        public async Task<List<ForecastVersion>> GetByProjectAsync(int projectId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM ForecastVersion WHERE ProjectId = @ProjectId ORDER BY VersionNumber";
            var result = await connection.QueryAsync<ForecastVersion>(sql, new { ProjectId = projectId });
            return result.ToList();
        }

        public async Task<int> CreateVersionAsync(ForecastVersion version)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO ForecastVersion (ProjectId, VersionNumber, CreatedAt)
                VALUES (@ProjectId, @VersionNumber, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            
            return await connection.ExecuteScalarAsync<int>(sql, version);
        }

        public async Task<List<ResourceAllocation>> GetAllocationsByVersionAsync(int forecastVersionId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM ResourceAllocation WHERE ForecastVersionId = @ForecastVersionId";
            var result = await connection.QueryAsync<ResourceAllocation>(sql, new { ForecastVersionId = forecastVersionId });
            return result.ToList();
        }

        public async Task UpsertAllocationAsync(ResourceAllocation allocation)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                IF EXISTS (SELECT 1 FROM ResourceAllocation WHERE ForecastVersionId = @ForecastVersionId AND RosterId = @RosterId AND Month = @Month)
                BEGIN
                    UPDATE ResourceAllocation SET 
                        AllocatedDays = @AllocatedDays,
                        UpdatedAt = GETUTCDATE()
                    WHERE ForecastVersionId = @ForecastVersionId AND RosterId = @RosterId AND Month = @Month
                END
                ELSE
                BEGIN
                    INSERT INTO ResourceAllocation (ForecastVersionId, RosterId, Month, AllocatedDays)
                    VALUES (@ForecastVersionId, @RosterId, @Month, @AllocatedDays)
                END";
            
            await connection.ExecuteAsync(sql, allocation);
        }

        public async Task<List<ResourceAllocation>> GetAllLatestAllocationsAsync()
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT a.* 
                FROM ResourceAllocation a
                JOIN ForecastVersion v ON a.ForecastVersionId = v.Id
                INNER JOIN (
                    SELECT ProjectId, MAX(VersionNumber) as MaxVer
                    FROM ForecastVersion
                    GROUP BY ProjectId
                ) latest ON v.ProjectId = latest.ProjectId AND v.VersionNumber = latest.MaxVer";
            var result = await connection.QueryAsync<ResourceAllocation>(sql);
            return result.ToList();
        }


        public async Task DeleteResourceAllocationsAsync(int forecastVersionId, int rosterId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "DELETE FROM ResourceAllocation WHERE ForecastVersionId = @ForecastVersionId AND RosterId = @RosterId";
            await connection.ExecuteAsync(sql, new { ForecastVersionId = forecastVersionId, RosterId = rosterId });
        }
    }
}
