using System.Threading.Tasks;
using Dapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Infrastructure.Persistence;

namespace ResourceManagement.Infrastructure.Persistence.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly DapperContext _context;

        public AuditRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, string changedBy)
        {
            var connection = _context.GetOpenConnection();
            const string sql = @"
                INSERT INTO AuditLogs (EntityName, EntityId, Action, OldValues, NewValues, ChangedBy)
                VALUES (@EntityName, @EntityId, @Action, @OldValues, @NewValues, @ChangedBy)";
            
            await connection.ExecuteAsync(sql, new 
            { 
                EntityName = entityName, 
                EntityId = entityId, 
                Action = action, 
                OldValues = oldValues, 
                NewValues = newValues, 
                ChangedBy = changedBy 
            });
        }

        public async Task<System.Collections.Generic.IEnumerable<Domain.Entities.AuditLog>> GetRecentAsync(int count)
        {
            var connection = _context.GetOpenConnection();
            const string sql = "SELECT TOP (@Count) * FROM AuditLogs ORDER BY ChangedAt DESC";
            return await connection.QueryAsync<Domain.Entities.AuditLog>(sql, new { Count = count });
        }
    }
}
