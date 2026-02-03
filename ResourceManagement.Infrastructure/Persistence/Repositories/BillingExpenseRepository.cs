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
    public class BillingRepository : IBillingRepository
    {
        private readonly DapperContext _context;

        public BillingRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<Billing>> GetByProjectAsync(int projectId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Billing WHERE ProjectId = @ProjectId ORDER BY Month";
            var result = await connection.QueryAsync<Billing>(sql, new { ProjectId = projectId });
            return result.ToList();
        }

        public async Task UpsertAsync(Billing billing)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                IF EXISTS (SELECT 1 FROM Billing WHERE ProjectId = @ProjectId AND Month = @Month)
                BEGIN
                    UPDATE Billing SET Amount = @Amount WHERE ProjectId = @ProjectId AND Month = @Month
                END
                ELSE
                BEGIN
                    INSERT INTO Billing (ProjectId, Month, Amount) VALUES (@ProjectId, @Month, @Amount)
                END";
            await connection.ExecuteAsync(sql, billing);
        }
    }

    public class ExpenseRepository : IExpenseRepository
    {
        private readonly DapperContext _context;

        public ExpenseRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<Expense>> GetByProjectAsync(int projectId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "SELECT * FROM Expense WHERE ProjectId = @ProjectId ORDER BY Month";
            var result = await connection.QueryAsync<Expense>(sql, new { ProjectId = projectId });
            return result.ToList();
        }

        public async Task UpsertAsync(Expense expense)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
                IF EXISTS (SELECT 1 FROM Expense WHERE ProjectId = @ProjectId AND Month = @Month)
                BEGIN
                    UPDATE Expense SET Amount = @Amount WHERE ProjectId = @ProjectId AND Month = @Month
                END
                ELSE
                BEGIN
                    INSERT INTO Expense (ProjectId, Month, Amount) VALUES (@ProjectId, @Month, @Amount)
                END";
            await connection.ExecuteAsync(sql, expense);
        }
    }
}
