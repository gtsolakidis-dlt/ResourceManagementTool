using System;

namespace ResourceManagement.Domain.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Wbs { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public decimal ActualBudget { get; set; } // Renamed from Budget
        public decimal NominalBudget { get; set; } // New
        public decimal Discount { get; set; } // Percentage (0-100)
        
        public decimal Recoverability { get; set; } // Percentage (0.00 to 1.00)
        public decimal TargetMargin { get; set; }   // Percentage (0.00 to 1.00)

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
