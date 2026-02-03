using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IProjectRateRepository
    {
        Task<List<ProjectRate>> GetByProjectAsync(int projectId);
        Task<ProjectRate?> GetByProjectAndLevelAsync(int projectId, string level);
        Task<int> CreateAsync(ProjectRate rate);
        Task UpdateAsync(ProjectRate rate);
        Task UpsertAsync(ProjectRate rate);
        Task DeleteByProjectAsync(int projectId);
        Task RecalculateRatesAsync(int projectId, decimal newDiscount);
        Task GenerateRatesForProjectAsync(int projectId, decimal discount);
    }
}
