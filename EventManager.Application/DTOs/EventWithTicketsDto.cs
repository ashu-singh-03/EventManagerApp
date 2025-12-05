using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class EventWithTicketsDto
    {
        public EventDto Event { get; set; }
        public List<TicketTypeDto> TicketTypes { get; set; } = new();
    }

}
