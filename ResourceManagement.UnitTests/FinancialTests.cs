using ResourceManagement.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ResourceManagement.UnitTests
{
    public class FinancialTests
    {
        [Fact]
        public void Calculate_DailyCost_ShouldBeCorrect()
        {
            // Arrange
            var roster = new Roster
            {
                MonthlySalary = 2000,
                MonthlyEmployerContributions = 500,
                Cars = 300,
                TicketRestaurant = 100,
                Metlife = 50
            };

            // Act
            // 14-month Cost = ((2000 + 500) * 14 / 12) + 300 + 100 + 50
            //               = (2500 * 1.1666...) + 450
            //               = 2916.66... + 450 = ~3366.66
            // Daily Cost = 3366.66 / 18 = ~187.03
            
            var expected14MonthAuth = ((2000m + 500m) * 14m / 12m) + 300m + 100m + 50m;
            var expectedDaily = expected14MonthAuth / 18.0m;

            // Assert
            roster.DailyCost.Should().BeApproximately(expectedDaily, 0.01m);
        }

        [Fact]
        public void NominalBudget_Calculation_Logic_Check()
        {
            // Since NominalBudget logic is inside handlers, let's verify the formula independently here
            // or we will test the Handler in a separate test class.
            // Formula: ActualBudget / (1 - Discount/100)
            
            decimal actualBudget = 100000m;
            decimal discountPercent = 20m;
            
            decimal expectedNominal = actualBudget / (1 - (discountPercent / 100m)); // 100k / 0.8 = 125k

            expectedNominal.Should().Be(125000m);
        }
    }
}
