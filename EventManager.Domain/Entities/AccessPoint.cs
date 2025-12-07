using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Domain.Entities
{
    public class AccessPoint
    {
        public int AccessPointId { get; set; }
        public int? EventId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Active { get; set; } = true;

        // Navigation property
        public Event Event { get; set; }
    }
}
