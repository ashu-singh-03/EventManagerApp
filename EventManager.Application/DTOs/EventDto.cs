using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string EventDate { get; set; }
        public string EventTime { get; set; }
        public string Location { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public int ParticipantCount { get; set; }
    }

}
