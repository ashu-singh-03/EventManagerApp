public class TicketTypeDto
{
    public int TicketTypeId { get; set; }
    public int EventId { get; set; }
    public string TicketName { get; set; }
    public decimal Price { get; set; }
    public int BookingTypeID { get; set; }
    public bool IsCapacityUnlimited { get; set; }

    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }

    public DateTime? SalesEndDate { get; set; }
    public string Description { get; set; }
    public bool IsFreeTicket { get; set; }
    public int UserId { get; set; } // for createdBy & modifiedBy
}
