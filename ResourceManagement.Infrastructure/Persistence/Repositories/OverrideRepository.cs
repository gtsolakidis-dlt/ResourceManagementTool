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
    public class OverrideRepository : IOverrideRepository
    {
        private readonly DapperContext _context;

        public OverrideRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<Override>> GetByProjectAsync(int projectId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM [Override] WHERE ProjectId = @ProjectId ORDER BY Month";
            var result = await connection.QueryAsync<Override>(sql, new { ProjectId = projectId });
            return result.ToList();
        }

        public async Task<Override?> GetByMonthAsync(int projectId, DateTime month)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM [Override] WHERE ProjectId = @ProjectId AND Month = @Month";
            return await connection.QuerySingleOrDefaultAsync<Override>(sql, new { ProjectId = projectId, Month = month });
        }

        public async Task<int> CreateAsync(Override @override)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO [Override] (
                    ProjectId, Month, Confirmed, OpeningBalance, Billings, Wip, Expenses, Cost, Nsr, Margin
                )
                VALUES (
                    @ProjectId, @Month, @Confirmed, @OpeningBalance, @Billings, @Wip, @Expenses, @Cost, @Nsr, @Margin
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";
            
            return await connection.ExecuteScalarAsync<int>(sql, @override);
        }

        public async Task UpdateAsync(Override @override)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE [Override] SET 
                    OpeningBalance = @OpeningBalance, 
                    Billings = @Billings, 
                    Wip = @Wip, 
                    Expenses = @Expenses, 
                    Cost = @Cost, 
                    Nsr = @Nsr, 
                    Margin = @Margin
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, @override);
        }

        public async Task ConfirmAsync(int id, string confirmedBy)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE [Override] SET 
                    Confirmed = 1, 
                    ConfirmedAt = GETUTCDATE(), 
                    ConfirmedBy = @ConfirmedBy 
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, new { Id = id, ConfirmedBy = confirmedBy });
        }
    }
}
