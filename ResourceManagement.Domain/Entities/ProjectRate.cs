using System;

namespace ResourceManagement.Domain.Entities
{
    /// <summary>
    /// Stores actual daily rates per project per level.
    /// ActualDailyRate = NominalRate * (1 - Discount/100)
    /// </summary>
    public class ProjectRate
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Level { get; set; } = string.Empty;
        public decimal NominalRate { get; set; }      // Snapshot from GlobalRate at project creation
        public decimal ActualDailyRate { get; set; }  // NominalRate * (1 - Discount/100)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
