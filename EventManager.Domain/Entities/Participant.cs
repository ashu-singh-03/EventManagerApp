namespace EventManager.Domain.Entities
{
    public class Participant
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
        public string QrCodeHash { get; set; }
        public DateTime QrCodeGeneratedAt { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        public Event Event { get; set; }
    }

}
