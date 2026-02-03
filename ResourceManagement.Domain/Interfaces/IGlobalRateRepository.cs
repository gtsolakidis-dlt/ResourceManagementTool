using ResourceManagement.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IGlobalRateRepository
    {
        Task<GlobalRate?> GetByIdAsync(int id);
        Task<GlobalRate?> GetByLevelAsync(string level);
        Task<List<GlobalRate>> GetAllAsync();
        Task<int> CreateAsync(GlobalRate globalRate);
        Task UpdateAsync(GlobalRate globalRate);
        Task DeleteAsync(int id);
    }
}
