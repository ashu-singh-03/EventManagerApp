using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Domain.Entities
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string Location { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public int ParticipantCount { get; set; }

    }

}
