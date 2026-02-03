using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IOverrideRepository
    {
        Task<List<Override>> GetByProjectAsync(int projectId);
        Task<Override?> GetByMonthAsync(int projectId, DateTime month);
        Task<int> CreateAsync(Override @override);
        Task UpdateAsync(Override @override);
        Task ConfirmAsync(int id, string confirmedBy);
    }
}
