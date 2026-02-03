using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IForecastRepository
    {
        Task<ForecastVersion?> GetVersionByIdAsync(int id);
        Task<List<ForecastVersion>> GetByProjectAsync(int projectId);
        Task<int> CreateVersionAsync(ForecastVersion version);
        
        Task<List<ResourceAllocation>> GetAllocationsByVersionAsync(int forecastVersionId);
        Task UpsertAllocationAsync(ResourceAllocation allocation);

        Task<List<ResourceAllocation>> GetAllLatestAllocationsAsync();
        Task DeleteResourceAllocationsAsync(int forecastVersionId, int rosterId);
    }
}
