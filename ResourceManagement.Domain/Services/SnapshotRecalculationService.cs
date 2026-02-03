using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Domain.Services
{
    public interface ISnapshotRecalculationService
    {
        /// <summary>
        /// Recalculate all non-Confirmed snapshots from a specific month onwards.
        /// Preserves overwritten values - only recalculates non-overridden fields.
        /// Opening Balance is propagated from the last confirmed/overridden month.
        /// </summary>
        Task RecalculateFromMonthAsync(int projectId, int forecastVersionId, DateTime fromMonth);

        /// <summary>
        /// Recalculate only the editable month.
        /// </summary>
        Task RecalculateEditableMonthAsync(int projectId, int forecastVersionId);
    }

    public class SnapshotRecalculationService : ISnapshotRecalculationService
    {
        private readonly IProjectMonthlySnapshotRepository _snapshotRepository;
        private readonly IFinancialCalculationService _calculationService;
        private readonly IProjectRepository _projectRepository;
        private readonly IForecastRepository _forecastRepository;
        private readonly IRosterRepository _rosterRepository;
        private readonly IProjectRateRepository _projectRateRepository;
        private readonly IBillingRepository _billingRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IOverrideRepository _overrideRepository;

        public SnapshotRecalculationService(
            IProjectMonthlySnapshotRepository snapshotRepository,
            IFinancialCalculationService calculationService,
            IProjectRepository projectRepository,
            IForecastRepository forecastRepository,
            IRosterRepository rosterRepository,
            IProjectRateRepository projectRateRepository,
            IBillingRepository billingRepository,
            IExpenseRepository expenseRepository,
            IOverrideRepository overrideRepository)
        {
            _snapshotRepository = snapshotRepository;
            _calculationService = calculationService;
            _projectRepository = projectRepository;
            _forecastRepository = forecastRepository;
            _rosterRepository = rosterRepository;
            _projectRateRepository = projectRateRepository;
            _billingRepository = billingRepository;
            _expenseRepository = expenseRepository;
            _overrideRepository = overrideRepository;
        }

        public async Task RecalculateEditableMonthAsync(int projectId, int forecastVersionId)
        {
            var editableSnapshot = await _snapshotRepository.GetEditableMonthAsync(projectId, forecastVersionId);
            if (editableSnapshot == null) return;

            await RecalculateFromMonthAsync(projectId, forecastVersionId, editableSnapshot.Month);
        }

        public async Task RecalculateFromMonthAsync(int projectId, int forecastVersionId, DateTime fromMonth)
        {
            // Get all required data
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return;

            var allocations = await _forecastRepository.GetAllocationsByVersionAsync(forecastVersionId);
            var rosterMembers = await _rosterRepository.GetByProjectVersionAsync(forecastVersionId);
            var projectRates = await _projectRateRepository.GetByProjectAsync(projectId);
            var billings = await _billingRepository.GetByProjectAsync(projectId);
            var expenses = await _expenseRepository.GetByProjectAsync(projectId);
            var overrides = await _overrideRepository.GetByProjectAsync(projectId);

            // Calculate fresh financial data using ProjectRates
            var calculatedData = _calculationService.CalculateMonthlyFinancialsWithProjectRates(
                project, allocations, rosterMembers, billings, expenses, overrides, projectRates);

            // Get all snapshots for this project/version to determine OB propagation source
            var allSnapshots = await _snapshotRepository.GetByProjectAsync(projectId, forecastVersionId);
            
            // Find the last confirmed or overridden snapshot before the fromMonth to get OB anchor
            var anchorSnapshot = allSnapshots
                .Where(s => s.Month < fromMonth && (s.Status == SnapshotStatus.Confirmed || s.IsOverridden))
                .OrderByDescending(s => s.Month)
                .FirstOrDefault();
            
            decimal propagatedOpeningBalance = anchorSnapshot?.OpeningBalance ?? 0;

            // Get existing snapshots that need to be updated (non-Confirmed from the given month)
            var snapshotsToUpdate = await _snapshotRepository.GetNonConfirmedFromMonthAsync(
                projectId, forecastVersionId, fromMonth);

            foreach (var snapshot in snapshotsToUpdate.OrderBy(s => s.Month))
            {
                // Find the calculated data for this month
                var calcData = calculatedData.FirstOrDefault(c =>
                    c.Month.Year == snapshot.Month.Year && c.Month.Month == snapshot.Month.Month);

                if (calcData == null) continue;

                // Design decision: Preserve overwritten values
                // Only update fields that haven't been manually overridden
                if (!snapshot.IsOverridden)
                {
                    // Use propagated OB from anchor or previous month with override
                    snapshot.OpeningBalance = propagatedOpeningBalance;
                    snapshot.CumulativeBillings = calcData.Billings;
                    snapshot.Wip = calcData.Wip;
                    snapshot.DirectExpenses = calcData.Expenses;
                    snapshot.OperationalCost = calcData.Cost;
                    snapshot.MonthlyBillings = calcData.MonthlyBillings;
                    snapshot.MonthlyExpenses = calcData.MonthlyExpenses;
                    snapshot.CumulativeExpenses = calcData.Expenses;
                    
                    // ALWAYS Recalculate NSR and Margin using the correct formula
                    // NSR = WIP + Cumulative Billings - Opening Balance - Direct Expenses
                    snapshot.Nsr = snapshot.Wip + snapshot.CumulativeBillings 
                                 - snapshot.OpeningBalance - snapshot.DirectExpenses;
                    
                    // Margin = (NSR - Operational Cost) / NSR
                    snapshot.Margin = snapshot.Nsr == 0 ? 0 
                                    : (snapshot.Nsr - snapshot.OperationalCost) / snapshot.Nsr;
                }
                else
                {
                    // Snapshot is overridden - only update non-overrideable fields
                    // Monthly values are always from actual data
                    snapshot.MonthlyBillings = calcData.MonthlyBillings;
                    snapshot.MonthlyExpenses = calcData.MonthlyExpenses;
                    
                    // STILL recalculate NSR and Margin based on overridden values
                    snapshot.Nsr = snapshot.Wip + snapshot.CumulativeBillings 
                                 - snapshot.OpeningBalance - snapshot.DirectExpenses;
                    snapshot.Margin = snapshot.Nsr == 0 ? 0 
                                    : (snapshot.Nsr - snapshot.OperationalCost) / snapshot.Nsr;
                    
                    // Update propagated OB from this overridden snapshot for future months
                    propagatedOpeningBalance = snapshot.OpeningBalance;
                }

                snapshot.UpdatedAt = DateTime.UtcNow;
                await _snapshotRepository.UpdateAsync(snapshot);
            }
        }
    }
}

