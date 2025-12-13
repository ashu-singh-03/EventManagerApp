using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class SaveAssignmentsDto
    {
        public int TicketTypeId { get; set; }
        public List<int> AccessPointIds { get; set; } = new List<int>();
    }
}
