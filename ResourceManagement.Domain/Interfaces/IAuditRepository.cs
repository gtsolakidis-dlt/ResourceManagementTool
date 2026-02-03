using System.Threading.Tasks;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IAuditRepository
    {
        Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, string changedBy);
        Task<System.Collections.Generic.IEnumerable<Entities.AuditLog>> GetRecentAsync(int count);
    }
}
