using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Infrastructure.Persistence;

namespace ResourceManagement.Infrastructure.Persistence.Repositories
{
    public class RosterRepository : IRosterRepository
    {
        private readonly DapperContext _context;

        public RosterRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Roster?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Roster WHERE Id = @Id AND IsDeleted = 0";
            return await connection.QuerySingleOrDefaultAsync<Roster>(sql, new { Id = id });
        }

        public async Task<Roster?> GetBySapCodeAsync(string sapCode)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Roster WHERE SapCode = @SapCode AND IsDeleted = 0";
            return await connection.QuerySingleOrDefaultAsync<Roster>(sql, new { SapCode = sapCode });
        }

        public async Task<Roster?> GetByUsernameAsync(string username)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Roster WHERE Username = @Username AND IsDeleted = 0";
            return await connection.QuerySingleOrDefaultAsync<Roster>(sql, new { Username = username });
        }

        public async Task<List<Roster>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Roster WHERE IsDeleted = 0 ORDER BY FullNameEn";
            var result = await connection.QueryAsync<Roster>(sql);
            return result.ToList();
        }

        public async Task<List<Roster>> SearchAsync(string? searchTerm, string? function, string? level, string? costCenter)
        {
            using var connection = _context.CreateConnection();
            var sql = new StringBuilder("SELECT * FROM Roster WHERE IsDeleted = 0");
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql.Append(" AND (FullNameEn LIKE @SearchTerm OR SapCode LIKE @SearchTerm OR LegalEntity LIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrWhiteSpace(function))
            {
                sql.Append(" AND FunctionBusinessUnit = @Function");
                parameters.Add("Function", function);
            }

            if (!string.IsNullOrWhiteSpace(level))
            {
                sql.Append(" AND Level = @Level");
                parameters.Add("Level", level);
            }

            if (!string.IsNullOrWhiteSpace(costCenter))
            {
                sql.Append(" AND CostCenterCode = @CostCenter");
                parameters.Add("CostCenter", costCenter);
            }

            sql.Append(" ORDER BY FullNameEn");

            var result = await connection.QueryAsync<Roster>(sql.ToString(), parameters);
            return result.ToList();
        }

        public async Task<int> CreateAsync(Roster roster)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO Roster (
                    SapCode, FullNameEn, LegalEntity, FunctionBusinessUnit, CostCenterCode, Level,
                    MonthlySalary, MonthlyEmployerContributions, Cars, TicketRestaurant, Metlife,
                    Role, Username, PasswordHash
                )
                VALUES (
                    @SapCode, @FullNameEn, @LegalEntity, @FunctionBusinessUnit, @CostCenterCode, @Level,
                    @MonthlySalary, @MonthlyEmployerContributions, @Cars, @TicketRestaurant, @Metlife,
                    @Role, @Username, @PasswordHash
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, roster);
        }

        public async Task UpdateAsync(Roster roster)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE Roster SET
                    SapCode = @SapCode,
                    FullNameEn = @FullNameEn,
                    LegalEntity = @LegalEntity,
                    FunctionBusinessUnit = @FunctionBusinessUnit,
                    CostCenterCode = @CostCenterCode,
                    Level = @Level,
                    MonthlySalary = @MonthlySalary,
                    MonthlyEmployerContributions = @MonthlyEmployerContributions,
                    Cars = @Cars,
                    TicketRestaurant = @TicketRestaurant,
                    Metlife = @Metlife,
                    Role = @Role,
                    Username = @Username,
                    PasswordHash = @PasswordHash,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, roster);
        }

        public async Task<List<Roster>> GetByProjectVersionAsync(int forecastVersionId)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT DISTINCT r.* 
                FROM Roster r
                INNER JOIN ResourceAllocation ra ON r.Id = ra.RosterId
                WHERE ra.ForecastVersionId = @ForecastVersionId AND r.IsDeleted = 0";
            
            var result = await connection.QueryAsync<Roster>(sql, new { ForecastVersionId = forecastVersionId });
            return result.ToList();
        }

        public async Task DeleteAsync(int id)

        {
            using var connection = _context.CreateConnection();
            const string sql = "UPDATE Roster SET IsDeleted = 1, UpdatedAt = GETUTCDATE() WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
