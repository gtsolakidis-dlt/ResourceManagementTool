using System.Collections.Generic;

namespace ResourceManagement.Contracts.Suggestions
{
    public class ResourceSuggestionDto
    {
        public int RosterId { get; set; }
        public string FullNameEn { get; set; } = string.Empty;
        public string? Level { get; set; }
        public string? FunctionBusinessUnit { get; set; }
        public string? TechnicalRole { get; set; }
        public string? SeniorityTier { get; set; }
        public decimal DailyCost { get; set; }

        // Availability (across all projects, latest versions)
        public decimal TotalAvailableDays { get; set; }
        public decimal TotalCapacityDays { get; set; }
        public decimal AvailabilityPercentage { get; set; }
        public List<MonthlyAvailabilityDto> MonthlyAvailability { get; set; } = new();

        // Budget Impact
        public decimal ProjectedCost { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal BudgetImpactPercentage { get; set; }
        public string BudgetFit { get; set; } = "within";
    }

    public class MonthlyAvailabilityDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal AllocatedDays { get; set; }
        public decimal AvailableDays { get; set; }
        public decimal CapacityDays { get; set; }
    }
}
