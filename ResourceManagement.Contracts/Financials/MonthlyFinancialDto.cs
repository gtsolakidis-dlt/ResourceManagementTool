using System;

namespace ResourceManagement.Contracts.Financials
{
    public class MonthlyFinancialDto
    {
        public DateTime Month { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal MonthlyBillings { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal Billings { get; set; }
        public decimal Wip { get; set; }
        public decimal Expenses { get; set; }
        public decimal Cost { get; set; }
        public decimal Nsr { get; set; }
        public decimal Margin { get; set; }
        public bool IsOverridden { get; set; }
    }
}
