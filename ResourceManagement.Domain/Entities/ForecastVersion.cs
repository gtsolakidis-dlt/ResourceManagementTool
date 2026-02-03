using System;

namespace ResourceManagement.Domain.Entities
{
    public class ForecastVersion
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
