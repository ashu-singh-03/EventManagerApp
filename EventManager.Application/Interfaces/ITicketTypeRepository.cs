using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface ITicketTypeRepository
    {
        Task<IEnumerable<TicketType>> GetTicketTypesByEventAsync(int eventId);
        Task<TicketType> GetTicketTypeByIdAsync(int ticketTypeId);
        Task SaveTicketTypeAsync(TicketType ticketType);
        Task DeleteTicketTypeAsync(int ticketTypeId);
    }
}
