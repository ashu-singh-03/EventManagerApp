using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class SaveParticipantsDto
    {
        public int TicketTypeId { get; set; }
        public List<int> ParticipantIds { get; set; }
    }
}
