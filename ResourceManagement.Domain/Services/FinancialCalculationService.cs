using System;
using System.Collections.Generic;
using System.Linq;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Financials;

namespace ResourceManagement.Domain.Services
{
    public class FinancialCalculationService : IFinancialCalculationService
    {
        /// <summary>
        /// Calculate monthly financials using GlobalRate and inline discount calculation.
        /// This is the legacy method maintained for backwards compatibility.
        /// </summary>
        public List<MonthlyFinancialDto> CalculateMonthlyFinancials(
            Project project,
            List<ResourceAllocation> allocations,
            List<Roster> rosterMembers,
            List<Billing> billings,
            List<Expense> expenses,
            List<Override> overrides,
            List<GlobalRate> globalRates)
        {
            // Step 1: Get month range for the project
            var months = GetMonthRange(project.StartDate, project.EndDate);

            // Step 2: Calculate base values (non-cumulative) for each month
            var baseValues = CalculateBaseValues(months, allocations, rosterMembers, billings, expenses, globalRates, project.Discount);

            // Step 3: Calculate cumulative values with override propagation
            return CalculateCumulativeValues(months, baseValues, overrides);
        }

        /// <summary>
        /// Calculate monthly financials using ProjectRate (pre-calculated actual daily rates).
        /// This is the new method that uses persisted project-specific rates.
        /// WIP = AllocatedDays * ActualDailyRate (from ProjectRate)
        /// </summary>
        public List<MonthlyFinancialDto> CalculateMonthlyFinancialsWithProjectRates(
            Project project,
            List<ResourceAllocation> allocations,
            List<Roster> rosterMembers,
            List<Billing> billings,
            List<Expense> expenses,
            List<Override> overrides,
            List<ProjectRate> projectRates)
        {
            // Step 1: Get month range for the project
            var months = GetMonthRange(project.StartDate, project.EndDate);

            // Step 2: Calculate base values using ProjectRate for WIP calculation
            var baseValues = CalculateBaseValuesWithProjectRates(months, allocations, rosterMembers, billings, expenses, projectRates);

            // Step 3: Calculate cumulative values with override propagation
            return CalculateCumulativeValues(months, baseValues, overrides);
        }

        /// <summary>
        /// Calculate base values using ProjectRate for WIP calculation.
        /// </summary>
        private Dictionary<DateTime, BaseValues> CalculateBaseValuesWithProjectRates(
            List<DateTime> months,
            List<ResourceAllocation> allocations,
            List<Roster> rosterMembers,
            List<Billing> billings,
            List<Expense> expenses,
            List<ProjectRate> projectRates)
        {
            var result = new Dictionary<DateTime, BaseValues>();
            var rosterLookup = rosterMembers.ToDictionary(r => r.Id);
            var rateLookup = projectRates.ToDictionary(r => r.Level, r => r.ActualDailyRate);

            foreach (var month in months)
            {
                var monthAllocations = allocations
                    .Where(a => a.Month.Year == month.Year && a.Month.Month == month.Month)
                    .ToList();

                decimal costBase = 0;
                decimal wipBase = 0;

                foreach (var allocation in monthAllocations)
                {
                    if (rosterLookup.TryGetValue(allocation.RosterId, out var roster))
                    {
                        // Cost: Allocated Days * Daily Cost (from Roster entity)
                        costBase += allocation.AllocatedDays * roster.DailyCost;

                        // WIP: Allocated Days * ActualDailyRate (from ProjectRate)
                        if (!string.IsNullOrEmpty(roster.Level) && rateLookup.TryGetValue(roster.Level, out var actualDailyRate))
                        {
                            wipBase += allocation.AllocatedDays * actualDailyRate;
                        }
                    }
                }

                decimal billingBase = billings
                    .FirstOrDefault(b => b.Month.Year == month.Year && b.Month.Month == month.Month)
                    ?.Amount ?? 0;

                decimal expenseBase = expenses
                    .FirstOrDefault(e => e.Month.Year == month.Year && e.Month.Month == month.Month)
                    ?.Amount ?? 0;

                result[month] = new BaseValues
                {
                    Cost = costBase,
                    Wip = wipBase,
                    Billing = billingBase,
                    Expense = expenseBase
                };
            }

            return result;
        }

        private List<DateTime> GetMonthRange(DateTime startDate, DateTime endDate)
        {
            var months = new List<DateTime>();
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            while (current <= end)
            {
                months.Add(current);
                current = current.AddMonths(1);
            }

            return months;
        }

        private Dictionary<DateTime, BaseValues> CalculateBaseValues(
            List<DateTime> months,
            List<ResourceAllocation> allocations,
            List<Roster> rosterMembers,
            List<Billing> billings,
            List<Expense> expenses,
            List<GlobalRate> globalRates,
            decimal discountPercentage)
        {
            var result = new Dictionary<DateTime, BaseValues>();
            var rosterLookup = rosterMembers.ToDictionary(r => r.Id);
            var ratesLookup = globalRates.ToDictionary(r => r.Level, r => r.NominalRate);

            foreach (var month in months)
            {
                var monthAllocations = allocations
                    .Where(a => a.Month.Year == month.Year && a.Month.Month == month.Month)
                    .ToList();
                
                decimal costBase = 0;
                decimal wipBase = 0;

                foreach (var allocation in monthAllocations)
                {
                    if (rosterLookup.TryGetValue(allocation.RosterId, out var roster))
                    {
                        // Cost: Allocated Days * Daily Cost
                        costBase += allocation.AllocatedDays * roster.DailyCost;
                        
                        // WIP: Allocated Days * Nominal Rate * (1 - Discount/100)
                        if (!string.IsNullOrEmpty(roster.Level) && ratesLookup.TryGetValue(roster.Level, out var nominalRate))
                        {
                            var dailyRevenue = nominalRate * (1 - (discountPercentage / 100m));
                            wipBase += allocation.AllocatedDays * dailyRevenue;
                        }
                    }
                }

                decimal billingBase = billings
                    .FirstOrDefault(b => b.Month.Year == month.Year && b.Month.Month == month.Month)
                    ?.Amount ?? 0;

                decimal expenseBase = expenses
                    .FirstOrDefault(e => e.Month.Year == month.Year && e.Month.Month == month.Month)
                    ?.Amount ?? 0;

                result[month] = new BaseValues
                {
                    Cost = costBase,
                    Wip = wipBase,
                    Billing = billingBase,
                    Expense = expenseBase
                };
            }

            return result;
        }

        private List<MonthlyFinancialDto> CalculateCumulativeValues(
            List<DateTime> months,
            Dictionary<DateTime, BaseValues> baseValues,
            List<Override> overrides)
        {
            var result = new List<MonthlyFinancialDto>();
            MonthlyFinancialDto? previousMonth = null;

            foreach (var month in months)
            {
                var baseValue = baseValues[month];
                var monthOverride = overrides
                    .FirstOrDefault(o => o.Month.Year == month.Year && o.Month.Month == month.Month && o.Confirmed);

                MonthlyFinancialDto current;

                if (monthOverride != null)
                {
                    // Use override values as anchor point
                    current = new MonthlyFinancialDto
                    {
                        Month = month,
                        OpeningBalance = monthOverride.OpeningBalance ?? 0,
                        MonthlyBillings = baseValue.Billing,
                        MonthlyExpenses = baseValue.Expense,
                        Billings = monthOverride.Billings ?? 0,
                        Wip = monthOverride.Wip ?? 0,
                        Expenses = monthOverride.Expenses ?? 0,
                        Cost = monthOverride.Cost ?? 0,
                        Nsr = monthOverride.Nsr ?? 0,
                        Margin = monthOverride.Margin ?? 0,
                        IsOverridden = true
                    };
                }
                else
                {
                    if (previousMonth == null)
                    {
                        // First month
                        current = new MonthlyFinancialDto
                        {
                            Month = month,
                            OpeningBalance = 0,
                            MonthlyBillings = baseValue.Billing,
                            MonthlyExpenses = baseValue.Expense,
                            Billings = baseValue.Billing,
                            Wip = baseValue.Wip,
                            Expenses = baseValue.Expense,
                            Cost = baseValue.Cost,
                            IsOverridden = false
                        };
                    }
                    else
                    {
                        // Subsequent months: add base values to previous month's cumulative totals
                        current = new MonthlyFinancialDto
                        {
                            Month = month,
                            OpeningBalance = previousMonth.OpeningBalance, // Carry forward
                            MonthlyBillings = baseValue.Billing,
                            MonthlyExpenses = baseValue.Expense,
                            Billings = previousMonth.Billings + baseValue.Billing,
                            Wip = previousMonth.Wip + baseValue.Wip,
                            Expenses = previousMonth.Expenses + baseValue.Expense,
                            Cost = previousMonth.Cost + baseValue.Cost,
                            IsOverridden = false
                        };
                    }

                    // Calculate NSR and Margin
                    current.Nsr = current.Wip + current.Billings - current.OpeningBalance - current.Expenses;
                    current.Margin = current.Nsr == 0 ? 0 : (current.Nsr - current.Cost) / current.Nsr;
                }

                result.Add(current);
                previousMonth = current;
            }

            return result;
        }

        private class BaseValues
        {
            public decimal Cost { get; set; }
            public decimal Wip { get; set; }
            public decimal Billing { get; set; }
            public decimal Expense { get; set; }
        }
    }
}
