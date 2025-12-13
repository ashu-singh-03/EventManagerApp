using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class TicketAccessPointAssignmentDto
    {
        public int AccessPointId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAssigned { get; set; }
    }
}
