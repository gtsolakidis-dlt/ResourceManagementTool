using System;

namespace ResourceManagement.Contracts.Project
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Wbs { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public decimal ActualBudget { get; set; }
        public decimal NominalBudget { get; set; }
        public decimal Discount { get; set; }
        
        public decimal Recoverability { get; set; }
        public decimal TargetMargin { get; set; }
        
        // RBAC: Indicates if current user can edit this project
        public bool CanEdit { get; set; } = false;
    }
}
