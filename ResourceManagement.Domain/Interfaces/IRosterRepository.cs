using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IRosterRepository
    {
        Task<Roster?> GetByIdAsync(int id);
        Task<Roster?> GetBySapCodeAsync(string sapCode);
        Task<Roster?> GetByUsernameAsync(string username);
        Task<List<Roster>> GetAllAsync();
        Task<List<Roster>> SearchAsync(string? searchTerm, string? function, string? level, string? costCenter);
        Task<int> CreateAsync(Roster roster);
        Task<List<Roster>> GetByProjectVersionAsync(int forecastVersionId);
        Task UpdateAsync(Roster roster);

        Task DeleteAsync(int id);
    }
}
