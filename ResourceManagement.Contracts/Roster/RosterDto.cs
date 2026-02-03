using System;

namespace ResourceManagement.Contracts.Roster
{
    public class RosterDto
    {
        public int Id { get; set; }
        public string SapCode { get; set; } = string.Empty;
        public string FullNameEn { get; set; } = string.Empty;
        public string? LegalEntity { get; set; }
        public string? FunctionBusinessUnit { get; set; }
        public string? CostCenterCode { get; set; }
        public string? Level { get; set; }

        public decimal MonthlySalary { get; set; }
        public decimal MonthlyEmployerContributions { get; set; } // Renamed from EmployerContributions
        public decimal Cars { get; set; }
        public decimal TicketRestaurant { get; set; }
        public decimal Metlife { get; set; }
        
        // Calculated
        public decimal MonthlyCost_12months { get; set; }
        public decimal MonthlyCost_14months { get; set; }
        public decimal DailyCost { get; set; }
        public decimal ProjectedRevenue { get; set; }

        public string Role { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
