using System;

namespace ResourceManagement.Domain.Entities
{
    public class GlobalRate
    {
        public int Id { get; set; }
        public string Level { get; set; } = string.Empty; // BA, C, SC...
        public decimal NominalRate { get; set; } // Daily Rate
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
