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
        public string EventDate { get; set; }  // string from JS
        public string EventTime { get; set; }  // string from JS
        public string Location { get; set; }
        public string EndDate { get; set; }    // string from JS
        public string EndTime { get; set; }    // string from JS
    }

}
