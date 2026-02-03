using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IBillingRepository
    {
        Task<List<Billing>> GetByProjectAsync(int projectId);
        Task UpsertAsync(Billing billing);
    }

    public interface IExpenseRepository
    {
        Task<List<Expense>> GetByProjectAsync(int projectId);
        Task UpsertAsync(Expense expense);
    }
}
