using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Services;

namespace ResourceManagement.UnitTests
{
    public class FinancialCalculationServiceTests
    {
        private readonly FinancialCalculationService _service;

        public FinancialCalculationServiceTests()
        {
            _service = new FinancialCalculationService();
        }

        [Fact]
        public void CalculateMonthlyFinancialsWithProjectRates_ShouldCalculateWipBasedOnProjectRates()
        {
            // Arrange
            var project = new Project 
            { 
                Id = 1, 
                StartDate = new DateTime(2026, 1, 1), 
                EndDate = new DateTime(2026, 1, 31) 
            };

            var roster = new Roster 
            { 
                Id = 1, 
                FullNameEn = "John Doe", 
                Level = "SC", 
                MonthlySalary = 2000,
                MonthlyEmployerContributions = 500,
                Cars = 0,
                Metlife = 0,
                TicketRestaurant = 0
                // DailyCost will be calculated internally by Roster, approx (2500*14/12)/18 = ~162
            };

            var allocations = new List<ResourceAllocation>
            {
                new ResourceAllocation 
                { 
                    RosterId = 1, 
                    Month = new DateTime(2026, 1, 1), 
                    AllocatedDays = 10 
                }
            };

            var projectRates = new List<ProjectRate>
            {
                new ProjectRate { Level = "SC", ActualDailyRate = 500 }
            };

            // Act
            var result = _service.CalculateMonthlyFinancialsWithProjectRates(
                project, 
                allocations, 
                new List<Roster> { roster }, 
                new List<Billing>(), 
                new List<Expense>(), 
                new List<Override>(), 
                projectRates
            );

            // Assert
            var jan = result[0];
            jan.Wip.Should().Be(10 * 500); // 5000
            jan.Cost.Should().BeApproximately(10 * roster.DailyCost, 0.1m);
        }

        [Fact]
        public void CalculateMonthlyFinancialsWithProjectRates_ShouldAggregateCumulatively()
        {
             // Arrange
            var project = new Project 
            { 
                Id = 1, 
                StartDate = new DateTime(2026, 1, 1), 
                EndDate = new DateTime(2026, 2, 28) 
            };

             var roster = new Roster { Id = 1, Level = "A" };
             var projectRates = new List<ProjectRate> { new ProjectRate { Level = "A", ActualDailyRate = 100 } };

             var allocations = new List<ResourceAllocation>
             {
                 new ResourceAllocation { RosterId = 1, Month = new DateTime(2026, 1, 1), AllocatedDays = 10 }, // WIP = 1000
                 new ResourceAllocation { RosterId = 1, Month = new DateTime(2026, 2, 1), AllocatedDays = 10 }  // WIP = 1000
             };

             // Act
             var result = _service.CalculateMonthlyFinancialsWithProjectRates(
                project, 
                allocations, 
                new List<Roster> { roster }, 
                new List<Billing>(), 
                new List<Expense>(), 
                new List<Override>(), 
                projectRates
            );

             // Assert
             // Jan
             result[0].Wip.Should().Be(1000);
             
             // Feb (Cumulative)
             result[1].Wip.Should().Be(2000);
        }

        [Fact]
        public void CalculateMonthlyFinancials_ShouldUseGlobalRatesAndDiscount()
        {
            // Test Legacy Method
            // Arrange
            var project = new Project 
            { 
                Id = 1, 
                StartDate = new DateTime(2026, 1, 1), 
                EndDate = new DateTime(2026, 1, 31),
                Discount = 10 // 10% Discount
            };

            var roster = new Roster { Id = 1, Level = "M" };
            var globalRates = new List<GlobalRate> { new GlobalRate { Level = "M", NominalRate = 1000 } };
            
            var allocations = new List<ResourceAllocation>
            {
                 new ResourceAllocation { RosterId = 1, Month = new DateTime(2026, 1, 1), AllocatedDays = 5 }
            };

            // Act
            var result = _service.CalculateMonthlyFinancials(
                project,
                allocations,
                new List<Roster> { roster },
                new List<Billing>(), 
                new List<Expense>(), 
                new List<Override>(),
                globalRates
            );

            // Assert
            // Rate = 1000 * (1 - 0.10) = 900
            // WIP = 5 * 900 = 4500
            result[0].Wip.Should().Be(4500);
        }

        [Fact]
        public void CalculateMonthlyFinancialsWithProjectRates_ShouldPropagateOverrides()
        {
            // Arrange
            var project = new Project { StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 2, 28) };
            
            // Base calculation would result in 0 WIP (no allocations)
            
            var overrides = new List<Override>
            {
                new Override 
                { 
                    Month = new DateTime(2026, 1, 1), 
                    Confirmed = true,
                    Wip = 5000,
                    OpeningBalance = 100,
                    Billings = 0,
                    Expenses = 0,
                    Cost = 1000
                }
            };

            // Act
            var result = _service.CalculateMonthlyFinancialsWithProjectRates(
                project,
                new List<ResourceAllocation>(),
                new List<Roster>(),
                new List<Billing>(),
                new List<Expense>(),
                overrides,
                new List<ProjectRate>()
            );

            // Assert
            // Jan (Overridden)
            result[0].Wip.Should().Be(5000);
            result[0].OpeningBalance.Should().Be(100);
            result[0].IsOverridden.Should().BeTrue();

            // Feb (Calculated based on Jan)
            // Feb Base = 0
            // Cumulative WIP = Jan.Wip + Feb.Base = 5000 + 0 = 5000
            result[1].Wip.Should().Be(5000); 
            // Opening Balance for Feb should be Jan's OB (carried forward logic in service seems to be: 
            // current.OpeningBalance = previousMonth.OpeningBalance; (Line 249 of Service)
            result[1].OpeningBalance.Should().Be(100); 
        }
    }
}
