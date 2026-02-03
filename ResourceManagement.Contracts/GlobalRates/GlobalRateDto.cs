using System;

namespace ResourceManagement.Contracts.GlobalRates
{
    public class GlobalRateDto
    {
        public int Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public decimal NominalRate { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
