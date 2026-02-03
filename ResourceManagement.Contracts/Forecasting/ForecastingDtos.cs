using System;

namespace ResourceManagement.Contracts.Forecasting
{
    public class ResourceAllocationDto
    {
        public int Id { get; set; }
        public int ForecastVersionId { get; set; }
        public int RosterId { get; set; }
        public DateTime Month { get; set; }
        public decimal AllocatedDays { get; set; }
    }

    public class ForecastVersionDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
