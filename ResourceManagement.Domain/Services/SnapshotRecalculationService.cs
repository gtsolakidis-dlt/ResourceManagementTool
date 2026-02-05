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
        /// <param name="resetOverrides">If true, clears manual overrides for the recalculated months (used when inputs change).</param>
        Task RecalculateFromMonthAsync(int projectId, int forecastVersionId, DateTime fromMonth, bool resetOverrides = false);

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

        public async Task RecalculateFromMonthAsync(int projectId, int forecastVersionId, DateTime fromMonth, bool resetOverrides = false)
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
            // This returns data for the entire project timeline
            var calculatedData = _calculationService.CalculateMonthlyFinancialsWithProjectRates(
                project, allocations, rosterMembers, billings, expenses, overrides, projectRates);

            // Get all snapshots to find the anchor
            var allSnapshots = await _snapshotRepository.GetByProjectAsync(projectId, forecastVersionId);
            
            // Get snapshots to update (All non-confirmed from the requested month)
            var snapshotsToUpdate = await _snapshotRepository.GetNonConfirmedFromMonthAsync(
                projectId, forecastVersionId, fromMonth);
            
            if (!snapshotsToUpdate.Any()) return;

            var orderedSnapshots = snapshotsToUpdate.OrderBy(s => s.Month).ToList();
            var firstUpdateMonth = orderedSnapshots.First().Month;

            // Initialize Deltas and OB propagation from Anchor
            decimal propagatedOpeningBalance = 0;
            decimal wipDelta = 0;
            decimal cbDelta = 0;
            decimal deDelta = 0;
            decimal ocDelta = 0;

            var anchorSnapshot = allSnapshots
                .Where(s => s.Month < firstUpdateMonth)
                .OrderByDescending(s => s.Month)
                .FirstOrDefault();

            if (anchorSnapshot != null)
            {
                propagatedOpeningBalance = anchorSnapshot.OpeningBalance;

                // Determine Deltas from Anchor (Snapshot Value - Raw Calculated Value)
                // If Anchor is Confirmed or Overridden, it sets the baseline for the future.
                var anchorCalc = calculatedData.FirstOrDefault(c => 
                    c.Month.Year == anchorSnapshot.Month.Year && c.Month.Month == anchorSnapshot.Month.Month);
                
                if (anchorCalc != null)
                {
                    // Delta = Actual(Stored) - Theoretical(Calculated)
                    wipDelta = anchorSnapshot.Wip - anchorCalc.Wip;
                    cbDelta = anchorSnapshot.CumulativeBillings - anchorCalc.Billings;
                    deDelta = anchorSnapshot.DirectExpenses - anchorCalc.Expenses;
                    ocDelta = anchorSnapshot.OperationalCost - anchorCalc.Cost;
                }
            }

            foreach (var snapshot in orderedSnapshots)
            {
                var calcData = calculatedData.FirstOrDefault(c =>
                    c.Month.Year == snapshot.Month.Year && c.Month.Month == snapshot.Month.Month);

                if (calcData == null) continue;

                if (resetOverrides && snapshot.IsOverridden)
                {
                    snapshot.IsOverridden = false;
                    snapshot.OverriddenBy = null;
                    snapshot.OverriddenAt = null;
                }

                if (snapshot.IsOverridden)
                {
                    // Snapshot is overridden - its values are fixed by the user.
                    // Recalculate Deltas for future propagation.
                    wipDelta = snapshot.Wip - calcData.Wip;
                    cbDelta = snapshot.CumulativeBillings - calcData.Billings;
                    deDelta = snapshot.DirectExpenses - calcData.Expenses;
                    ocDelta = snapshot.OperationalCost - calcData.Cost;
                    
                    // Update OB propagation source
                    propagatedOpeningBalance = snapshot.OpeningBalance;
                    
                    // Update monthly flow values even if overridden (usually these aren't overridden directly in current UI, but if they were...)
                    // Current UI overrides Wip, CB, DE, OC. 
                    // MonthlyBillings/MonthlyExpenses are derived or separate.
                    // We update them from calcData as they are informational ? 
                    // Existing logic updated them.
                    snapshot.MonthlyBillings = calcData.MonthlyBillings;
                    snapshot.MonthlyExpenses = calcData.MonthlyExpenses;
                }
                else
                {
                    // Not overridden: Apply Calculated Values + Deltas
                    
                    // Propagate OB
                    snapshot.OpeningBalance = propagatedOpeningBalance;

                    // Apply Deltas to Cumulative/Stock fields
                    snapshot.Wip = calcData.Wip + wipDelta;
                    snapshot.CumulativeBillings = calcData.Billings + cbDelta;
                    snapshot.DirectExpenses = calcData.Expenses + deDelta;
                    snapshot.OperationalCost = calcData.Cost + ocDelta;
                    
                    snapshot.MonthlyBillings = calcData.MonthlyBillings;
                    snapshot.MonthlyExpenses = calcData.MonthlyExpenses;
                    snapshot.CumulativeExpenses = calcData.Expenses;
                }

                // ALWAYS Recalculate NSR and Margin
                // NSR = WIP + Cumulative Billings - Opening Balance - Direct Expenses
                snapshot.Nsr = snapshot.Wip + snapshot.CumulativeBillings 
                             - snapshot.OpeningBalance - snapshot.DirectExpenses;
                
                // Margin = (NSR - Operational Cost) / Abs(NSR)
                // Use Math.Abs for denominator to handle negative NSR correctly (e.g. NSR -100, Cost 50 -> Margin -1.5)
                snapshot.Margin = snapshot.Nsr == 0 ? 0 
                                : (snapshot.Nsr - snapshot.OperationalCost) / Math.Abs(snapshot.Nsr);

                snapshot.UpdatedAt = DateTime.UtcNow;
                await _snapshotRepository.UpdateAsync(snapshot);
                
                // Update propagated OB for next iteration (if not overridden, this month passes its OB forward)
                // (Matches the "Constant OB" logic unless changed)
                if (!snapshot.IsOverridden)
                {
                     propagatedOpeningBalance = snapshot.OpeningBalance;
                }
            }
        }
    }
}

