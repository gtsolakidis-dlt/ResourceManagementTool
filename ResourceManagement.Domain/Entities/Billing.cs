using System;

namespace ResourceManagement.Domain.Entities
{
    public class Billing
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public DateTime Month { get; set; }
        public decimal Amount { get; set; }
    }
}
