using System;

namespace ResourceManagement.Domain.Entities
{
    public class Roster
    {
        public int Id { get; set; }
        public string SapCode { get; set; } = string.Empty;
        public string FullNameEn { get; set; } = string.Empty;
        public string? LegalEntity { get; set; }
        public string? FunctionBusinessUnit { get; set; }
        public string? CostCenterCode { get; set; }
        public string? Level { get; set; }
        public string? TechnicalRole { get; set; }

        // Cost fields (EUR)
        public decimal MonthlySalary { get; set; } // Renamed from NewAmendedSalary
        public decimal MonthlyEmployerContributions { get; set; } // Renamed from EmployerContributions
        public decimal Cars { get; set; }
        public decimal TicketRestaurant { get; set; }
        public decimal Metlife { get; set; }

        // Calculated Properties for Greek Labor Laws (14 salaries)
        // MonthlyCost_12Months: Sum of (Salary + MonthlyEmployerContributions + Cars + TicketRestaurant + Metlife)
        public decimal MonthlyCost_12months => MonthlySalary + MonthlyEmployerContributions + Cars + TicketRestaurant + Metlife;

        // MonthlyCost_14Months: Accounts for 14-salary structure (2 additional salaries per year)
        // Formula: ((Salary + MonthlyEmployerContributions) * 14/12) + Cars + TicketRestaurant + Metlife
        public decimal MonthlyCost_14months => ((MonthlySalary + MonthlyEmployerContributions) * 14.0m / 12.0m) + Cars + TicketRestaurant + Metlife;

        // DailyCost: Fixed formula = MonthlyCost_14Months / 18 working days
        public decimal DailyCost => MonthlyCost_14months / 18.0m;
        
        // Removed Revenue/Topus fields

        // RBAC
        public string Role { get; set; } = "Employee";
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
