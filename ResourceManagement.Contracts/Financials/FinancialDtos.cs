using System;

namespace ResourceManagement.Contracts.Financials
{
    public class BillingDto
    {
        public int ProjectId { get; set; }
        public DateTime Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExpenseDto
    {
        public int ProjectId { get; set; }
        public DateTime Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class OverrideDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public DateTime Month { get; set; }
        public bool Confirmed { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? Billings { get; set; }
        public decimal? Wip { get; set; }
        public decimal? Expenses { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Nsr { get; set; }
        public decimal? Margin { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? ConfirmedBy { get; set; }
    }
}
