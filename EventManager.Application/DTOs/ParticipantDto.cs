using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class ParticipantDto
    {
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }      
        public string? Department { get; set; }   
        public string? Notes { get; set; }
        public string participants_code { get; set; }
    }
}

