using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Infrastructure.Persistence.Repositories
{
    public class ProjectMonthlySnapshotRepository : IProjectMonthlySnapshotRepository
    {
        private readonly DapperContext _context;

        public ProjectMonthlySnapshotRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectMonthlySnapshot>> GetByProjectAsync(int projectId, int forecastVersionId)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT * FROM ProjectMonthlySnapshot
                WHERE ProjectId = @ProjectId AND ForecastVersionId = @ForecastVersionId
                ORDER BY Month";

            var result = await connection.QueryAsync<ProjectMonthlySnapshot>(sql,
                new { ProjectId = projectId, ForecastVersionId = forecastVersionId });
            return result.ToList();
        }

        public async Task<ProjectMonthlySnapshot?> GetByMonthAsync(int projectId, int forecastVersionId, DateTime month)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT * FROM ProjectMonthlySnapshot
                WHERE ProjectId = @ProjectId
                  AND ForecastVersionId = @ForecastVersionId
                  AND YEAR(Month) = @Year AND MONTH(Month) = @MonthNum";

            return await connection.QuerySingleOrDefaultAsync<ProjectMonthlySnapshot>(sql,
                new { ProjectId = projectId, ForecastVersionId = forecastVersionId, Year = month.Year, MonthNum = month.Month });
        }

        public async Task<ProjectMonthlySnapshot?> GetEditableMonthAsync(int projectId, int forecastVersionId)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT * FROM ProjectMonthlySnapshot
                WHERE ProjectId = @ProjectId
                  AND ForecastVersionId = @ForecastVersionId
                  AND Status = 1"; // 1 = Editable

            return await connection.QuerySingleOrDefaultAsync<ProjectMonthlySnapshot>(sql,
                new { ProjectId = projectId, ForecastVersionId = forecastVersionId });
        }

        public async Task<List<ProjectMonthlySnapshot>> GetNonConfirmedFromMonthAsync(int projectId, int forecastVersionId, DateTime fromMonth)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT * FROM ProjectMonthlySnapshot
                WHERE ProjectId = @ProjectId
                  AND ForecastVersionId = @ForecastVersionId
                  AND Status != 2
                  AND Month >= @FromMonth
                ORDER BY Month";

            var result = await connection.QueryAsync<ProjectMonthlySnapshot>(sql,
                new { ProjectId = projectId, ForecastVersionId = forecastVersionId, FromMonth = new DateTime(fromMonth.Year, fromMonth.Month, 1) });
            return result.ToList();
        }

        public async Task<int> CreateAsync(ProjectMonthlySnapshot snapshot)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO ProjectMonthlySnapshot (
                    ProjectId, ForecastVersionId, Month, Status,
                    OpeningBalance, CumulativeBillings, Wip, DirectExpenses, OperationalCost,
                    MonthlyBillings, MonthlyExpenses, CumulativeExpenses, Nsr, Margin,
                    OriginalOpeningBalance, OriginalCumulativeBillings, OriginalWip, OriginalDirectExpenses, OriginalOperationalCost,
                    IsOverridden, OverriddenAt, OverriddenBy,
                    ConfirmedAt, ConfirmedBy
                )
                VALUES (
                    @ProjectId, @ForecastVersionId, @Month, @Status,
                    @OpeningBalance, @CumulativeBillings, @Wip, @DirectExpenses, @OperationalCost,
                    @MonthlyBillings, @MonthlyExpenses, @CumulativeExpenses, @Nsr, @Margin,
                    @OriginalOpeningBalance, @OriginalCumulativeBillings, @OriginalWip, @OriginalDirectExpenses, @OriginalOperationalCost,
                    @IsOverridden, @OverriddenAt, @OverriddenBy,
                    @ConfirmedAt, @ConfirmedBy
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, snapshot);
        }

        public async Task UpdateAsync(ProjectMonthlySnapshot snapshot)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE ProjectMonthlySnapshot SET
                    Status = @Status,
                    OpeningBalance = @OpeningBalance,
                    CumulativeBillings = @CumulativeBillings,
                    Wip = @Wip,
                    DirectExpenses = @DirectExpenses,
                    OperationalCost = @OperationalCost,
                    MonthlyBillings = @MonthlyBillings,
                    MonthlyExpenses = @MonthlyExpenses,
                    CumulativeExpenses = @CumulativeExpenses,
                    Nsr = @Nsr,
                    Margin = @Margin,
                    OriginalOpeningBalance = @OriginalOpeningBalance,
                    OriginalCumulativeBillings = @OriginalCumulativeBillings,
                    OriginalWip = @OriginalWip,
                    OriginalDirectExpenses = @OriginalDirectExpenses,
                    OriginalOperationalCost = @OriginalOperationalCost,
                    IsOverridden = @IsOverridden,
                    OverriddenAt = @OverriddenAt,
                    OverriddenBy = @OverriddenBy,
                    ConfirmedAt = @ConfirmedAt,
                    ConfirmedBy = @ConfirmedBy,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, snapshot);
        }

        public async Task UpsertAsync(ProjectMonthlySnapshot snapshot)
        {
            using var connection = _context.CreateConnection();

            // Check if exists
            var existing = await GetByMonthAsync(snapshot.ProjectId, snapshot.ForecastVersionId, snapshot.Month);

            if (existing != null)
            {
                snapshot.Id = existing.Id;
                await UpdateAsync(snapshot);
            }
            else
            {
                await CreateAsync(snapshot);
            }
        }

        public async Task<bool> ConfirmMonthAsync(int id, string confirmedBy)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE ProjectMonthlySnapshot
                SET Status = 2,
                    ConfirmedAt = GETUTCDATE(),
                    ConfirmedBy = @ConfirmedBy,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id AND Status = 1"; // Only confirm if currently Editable

            var affected = await connection.ExecuteAsync(sql, new { Id = id, ConfirmedBy = confirmedBy });
            return affected > 0;
        }

        public async Task<bool> PromoteNextPendingToEditableAsync(int projectId, int forecastVersionId)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE ProjectMonthlySnapshot
                SET Status = 1, UpdatedAt = GETUTCDATE()
                WHERE Id = (
                    SELECT TOP 1 Id
                    FROM ProjectMonthlySnapshot
                    WHERE ProjectId = @ProjectId
                      AND ForecastVersionId = @ForecastVersionId
                      AND Status = 0
                    ORDER BY Month
                )";

            var affected = await connection.ExecuteAsync(sql,
                new { ProjectId = projectId, ForecastVersionId = forecastVersionId });
            return affected > 0;
        }

        public async Task InitializeSnapshotsForProjectAsync(int projectId, int forecastVersionId, DateTime startDate, DateTime endDate)
        {
            using var connection = _context.CreateConnection();

            var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
            var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
            bool isFirstMonth = true;

            while (currentMonth <= endMonth)
            {
                // First month = Editable (1), rest = Pending (0)
                var status = isFirstMonth ? 1 : 0;

                const string sql = @"
                    IF NOT EXISTS (
                        SELECT 1 FROM ProjectMonthlySnapshot
                        WHERE ProjectId = @ProjectId
                          AND ForecastVersionId = @ForecastVersionId
                          AND YEAR(Month) = @Year AND MONTH(Month) = @MonthNum
                    )
                    BEGIN
                        INSERT INTO ProjectMonthlySnapshot (ProjectId, ForecastVersionId, Month, Status)
                        VALUES (@ProjectId, @ForecastVersionId, @Month, @Status)
                    END";

                await connection.ExecuteAsync(sql, new
                {
                    ProjectId = projectId,
                    ForecastVersionId = forecastVersionId,
                    Month = currentMonth,
                    Year = currentMonth.Year,
                    MonthNum = currentMonth.Month,
                    Status = status
                });

                currentMonth = currentMonth.AddMonths(1);
                isFirstMonth = false;
            }

            // Ensure at least one month is Editable if none exists
            const string ensureEditableSql = @"
                IF NOT EXISTS (
                    SELECT 1 FROM ProjectMonthlySnapshot 
                    WHERE ProjectId = @ProjectId 
                      AND ForecastVersionId = @ForecastVersionId 
                      AND Status = 1
                )
                BEGIN
                    UPDATE ProjectMonthlySnapshot 
                    SET Status = 1 
                    WHERE Id = (
                        SELECT TOP 1 Id 
                        FROM ProjectMonthlySnapshot 
                        WHERE ProjectId = @ProjectId 
                          AND ForecastVersionId = @ForecastVersionId 
                          AND Status = 0 
                        ORDER BY Month
                    )
                END";
            await connection.ExecuteAsync(ensureEditableSql, new { ProjectId = projectId, ForecastVersionId = forecastVersionId });
        }

        public async Task DeleteByForecastVersionAsync(int forecastVersionId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "DELETE FROM ProjectMonthlySnapshot WHERE ForecastVersionId = @ForecastVersionId";
            await connection.ExecuteAsync(sql, new { ForecastVersionId = forecastVersionId });
        }
    }
}
