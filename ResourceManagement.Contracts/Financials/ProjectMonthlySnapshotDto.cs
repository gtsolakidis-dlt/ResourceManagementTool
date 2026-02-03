using System;

namespace ResourceManagement.Contracts.Financials
{
    /// <summary>
    /// DTO for Project Monthly Snapshot (persisted financial state with workflow status)
    /// </summary>
    public class ProjectMonthlySnapshotDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int ForecastVersionId { get; set; }
        public DateTime Month { get; set; }

        /// <summary>
        /// Status: "Pending", "Editable", or "Confirmed"
        /// </summary>
        public string Status { get; set; } = string.Empty;

        // Financial Values (OB, CB, WIP, DE, OC)
        public decimal OpeningBalance { get; set; }
        public decimal CumulativeBillings { get; set; }
        public decimal Wip { get; set; }
        public decimal DirectExpenses { get; set; }
        public decimal OperationalCost { get; set; }

        // Additional metrics
        public decimal MonthlyBillings { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal CumulativeExpenses { get; set; }
        public decimal Nsr { get; set; }
        public decimal Margin { get; set; }

        // Original values (prior into override)
        public decimal? OriginalOpeningBalance { get; set; }
        public decimal? OriginalCumulativeBillings { get; set; }
        public decimal? OriginalWip { get; set; }
        public decimal? OriginalDirectExpenses { get; set; }
        public decimal? OriginalOperationalCost { get; set; }

        // Override tracking
        public bool IsOverridden { get; set; }
        public DateTime? OverriddenAt { get; set; }
        public string? OverriddenBy { get; set; }

        // Confirmation tracking
        public DateTime? ConfirmedAt { get; set; }
        public string? ConfirmedBy { get; set; }
    }
}
