using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class ParticipantCommunicationDto
    {
        public int ParticipantId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Company { get; set; }
        public int TicketTypeId { get; set; }
        public string TicketTypes { get; set; }
        public string AccessPoints { get; set; }
        public string? ParticipantCode { get; set; }
    }
}
