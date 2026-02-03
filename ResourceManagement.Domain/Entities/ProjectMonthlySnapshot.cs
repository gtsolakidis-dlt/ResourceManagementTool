using System;

namespace ResourceManagement.Domain.Entities
{
    /// <summary>
    /// Snapshot status for monthly financial workflow
    /// </summary>
    public enum SnapshotStatus
    {
        Pending = 0,
        Editable = 1,
        Confirmed = 2
    }

    /// <summary>
    /// Persisted monthly financial state with workflow status.
    /// Calculations for OB, CB, WIP, DE, OC are triggered and stored
    /// when input changes to Billings, Expenses, or Forecast Allocation.
    /// </summary>
    public class ProjectMonthlySnapshot
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int ForecastVersionId { get; set; }
        public DateTime Month { get; set; }

        /// <summary>
        /// Status: Pending, Editable, or Confirmed.
        /// Only ONE month can be Editable at a time per project/version.
        /// </summary>
        public SnapshotStatus Status { get; set; } = SnapshotStatus.Pending;

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

        // Original values (prior to override)
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

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
