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
    public class ProjectRepository : IProjectRepository
    {
        private readonly DapperContext _context;

        public ProjectRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Project WHERE Id = @Id AND IsDeleted = 0";
            return await connection.QuerySingleOrDefaultAsync<Project>(sql, new { Id = id });
        }

        public async Task<Project?> GetByWbsAsync(string wbs)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Project WHERE Wbs = @Wbs AND IsDeleted = 0";
            return await connection.QuerySingleOrDefaultAsync<Project>(sql, new { Wbs = wbs });
        }

        public async Task<List<Project>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Project WHERE IsDeleted = 0 ORDER BY Name";
            var result = await connection.QueryAsync<Project>(sql);
            return result.ToList();
        }

        public async Task<List<Project>> GetByResourceIdAsync(int rosterId)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT DISTINCT p.* 
                FROM Project p 
                INNER JOIN ForecastVersion fv ON p.Id = fv.ProjectId
                INNER JOIN ResourceAllocation ra ON fv.Id = ra.ForecastVersionId 
                WHERE ra.RosterId = @RosterId AND p.IsDeleted = 0 
                ORDER BY p.Name";
            var result = await connection.QueryAsync<Project>(sql, new { RosterId = rosterId });
            return result.ToList();
        }

        public async Task<bool> IsUserAssignedToProjectAsync(int rosterId, int projectId)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                SELECT COUNT(1) 
                FROM ForecastVersion fv
                INNER JOIN ResourceAllocation ra ON fv.Id = ra.ForecastVersionId 
                WHERE fv.ProjectId = @ProjectId AND ra.RosterId = @RosterId";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { RosterId = rosterId, ProjectId = projectId });
            return count > 0;
        }

        public async Task<int> CreateAsync(Project project)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                INSERT INTO Project (Name, Wbs, StartDate, EndDate, ActualBudget, NominalBudget, Discount, Recoverability, TargetMargin)
                VALUES (@Name, @Wbs, @StartDate, @EndDate, @ActualBudget, @NominalBudget, @Discount, @Recoverability, @TargetMargin);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            
            return await connection.ExecuteScalarAsync<int>(sql, project);
        }

        public async Task UpdateAsync(Project project)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                UPDATE Project SET
                    Name = @Name,
                    Wbs = @Wbs,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    ActualBudget = @ActualBudget,
                    NominalBudget = @NominalBudget,
                    Discount = @Discount,
                    Recoverability = @Recoverability,
                    TargetMargin = @TargetMargin,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, project);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            const string sql = "UPDATE Project SET IsDeleted = 1, UpdatedAt = GETUTCDATE() WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
