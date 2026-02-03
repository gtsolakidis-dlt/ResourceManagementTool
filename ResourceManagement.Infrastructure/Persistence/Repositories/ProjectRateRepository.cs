using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Infrastructure.Persistence.Repositories
{
    public class ProjectRateRepository : IProjectRateRepository
    {
        private readonly DapperContext _context;
        private readonly IGlobalRateRepository _globalRateRepository;

        public ProjectRateRepository(DapperContext context, IGlobalRateRepository globalRateRepository)
        {
            _context = context;
            _globalRateRepository = globalRateRepository;
        }

        public async Task<List<ProjectRate>> GetByProjectAsync(int projectId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM ProjectRate WHERE ProjectId = @ProjectId ORDER BY Level";
            var result = await connection.QueryAsync<ProjectRate>(sql, new { ProjectId = projectId });
            return result.ToList();
        }

        public async Task<ProjectRate?> GetByProjectAndLevelAsync(int projectId, string level)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM ProjectRate WHERE ProjectId = @ProjectId AND Level = @Level";
            return await connection.QuerySingleOrDefaultAsync<ProjectRate>(sql, new { ProjectId = projectId, Level = level });
        }

        public async Task<int> CreateAsync(ProjectRate rate)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO ProjectRate (ProjectId, Level, NominalRate, ActualDailyRate)
                VALUES (@ProjectId, @Level, @NominalRate, @ActualDailyRate);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, rate);
        }

        public async Task UpdateAsync(ProjectRate rate)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE ProjectRate SET
                    NominalRate = @NominalRate,
                    ActualDailyRate = @ActualDailyRate,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, rate);
        }

        public async Task UpsertAsync(ProjectRate rate)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                IF EXISTS (SELECT 1 FROM ProjectRate WHERE ProjectId = @ProjectId AND Level = @Level)
                BEGIN
                    UPDATE ProjectRate SET
                        NominalRate = @NominalRate,
                        ActualDailyRate = @ActualDailyRate,
                        UpdatedAt = GETUTCDATE()
                    WHERE ProjectId = @ProjectId AND Level = @Level
                END
                ELSE
                BEGIN
                    INSERT INTO ProjectRate (ProjectId, Level, NominalRate, ActualDailyRate)
                    VALUES (@ProjectId, @Level, @NominalRate, @ActualDailyRate)
                END";

            await connection.ExecuteAsync(sql, rate);
        }

        public async Task DeleteByProjectAsync(int projectId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "DELETE FROM ProjectRate WHERE ProjectId = @ProjectId";
            await connection.ExecuteAsync(sql, new { ProjectId = projectId });
        }

        public async Task RecalculateRatesAsync(int projectId, decimal newDiscount)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE ProjectRate
                SET ActualDailyRate = NominalRate * (1 - @Discount / 100.0),
                    UpdatedAt = GETUTCDATE()
                WHERE ProjectId = @ProjectId";

            await connection.ExecuteAsync(sql, new { ProjectId = projectId, Discount = newDiscount });
        }

        public async Task GenerateRatesForProjectAsync(int projectId, decimal discount)
        {
            // Get all global rates
            var globalRates = await _globalRateRepository.GetAllAsync();

            using var connection = _context.CreateConnection();

            foreach (var globalRate in globalRates)
            {
                var actualDailyRate = globalRate.NominalRate * (1 - discount / 100m);

                const string sql = @"
                    IF NOT EXISTS (SELECT 1 FROM ProjectRate WHERE ProjectId = @ProjectId AND Level = @Level)
                    BEGIN
                        INSERT INTO ProjectRate (ProjectId, Level, NominalRate, ActualDailyRate)
                        VALUES (@ProjectId, @Level, @NominalRate, @ActualDailyRate)
                    END
                    ELSE
                    BEGIN
                        UPDATE ProjectRate SET
                            NominalRate = @NominalRate,
                            ActualDailyRate = @ActualDailyRate,
                            UpdatedAt = GETUTCDATE()
                        WHERE ProjectId = @ProjectId AND Level = @Level
                    END";

                await connection.ExecuteAsync(sql, new
                {
                    ProjectId = projectId,
                    Level = globalRate.Level,
                    NominalRate = globalRate.NominalRate,
                    ActualDailyRate = actualDailyRate
                });
            }
        }
    }
}
