using Dapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManagement.Infrastructure.Persistence.Repositories
{
    public class GlobalRateRepository : IGlobalRateRepository
    {
        private readonly DapperContext _context;

        public GlobalRateRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<GlobalRate?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM GlobalRate WHERE Id = @Id";
            return await connection.QuerySingleOrDefaultAsync<GlobalRate>(sql, new { Id = id });
        }

        public async Task<GlobalRate?> GetByLevelAsync(string level)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM GlobalRate WHERE Level = @Level";
            return await connection.QuerySingleOrDefaultAsync<GlobalRate>(sql, new { Level = level });
        }

        public async Task<List<GlobalRate>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM GlobalRate ORDER BY Level";
            var result = await connection.QueryAsync<GlobalRate>(sql);
            return result.ToList();
        }

        public async Task<int> CreateAsync(GlobalRate globalRate)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO GlobalRate (Level, NominalRate, UpdatedAt)
                VALUES (@Level, @NominalRate, GETUTCDATE());
                SELECT CAST(SCOPE_IDENTITY() as int);";
            
            return await connection.ExecuteScalarAsync<int>(sql, globalRate);
        }

        public async Task UpdateAsync(GlobalRate globalRate)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE GlobalRate SET
                    Level = @Level,
                    NominalRate = @NominalRate,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, globalRate);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            const string sql = "DELETE FROM GlobalRate WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
