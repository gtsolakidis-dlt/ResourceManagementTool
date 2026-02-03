using System.Collections.Generic;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Contracts.Financials;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IFinancialCalculationService
    {
        /// <summary>
        /// Calculate monthly financials using GlobalRate and inline discount calculation.
        /// Legacy method maintained for backwards compatibility.
        /// </summary>
        List<MonthlyFinancialDto> CalculateMonthlyFinancials(
            Project project,
            List<ResourceAllocation> allocations,
            List<Roster> rosterMembers,
            List<Billing> billings,
            List<Expense> expenses,
            List<Override> overrides,
            List<GlobalRate> globalRates);

        /// <summary>
        /// Calculate monthly financials using ProjectRate (pre-calculated actual daily rates).
        /// WIP = AllocatedDays * ActualDailyRate (from ProjectRate)
        /// </summary>
        List<MonthlyFinancialDto> CalculateMonthlyFinancialsWithProjectRates(
            Project project,
            List<ResourceAllocation> allocations,
            List<Roster> rosterMembers,
            List<Billing> billings,
            List<Expense> expenses,
            List<Override> overrides,
            List<ProjectRate> projectRates);
    }
}
