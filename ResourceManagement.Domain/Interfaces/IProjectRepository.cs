using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(int id);
        Task<Project?> GetByWbsAsync(string wbs);
        Task<List<Project>> GetAllAsync();
        Task<List<Project>> GetByResourceIdAsync(int rosterId);
        Task<bool> IsUserAssignedToProjectAsync(int rosterId, int projectId);
        Task<int> CreateAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(int id);
    }
}
