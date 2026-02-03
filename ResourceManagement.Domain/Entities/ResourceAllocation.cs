using System;

namespace ResourceManagement.Domain.Entities
{
    public class ResourceAllocation
    {
        public int Id { get; set; }
        public int ForecastVersionId { get; set; }
        public int RosterId { get; set; }
        public DateTime Month { get; set; } // Represented as first day of month
        public decimal AllocatedDays { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
