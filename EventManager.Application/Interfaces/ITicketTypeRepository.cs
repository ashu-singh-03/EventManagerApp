using EventManager.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Application.Interfaces
{
    public interface ITicketTypeRepository
    {
        // Get all ticket types for a specific event
        Task<List<TicketType>> GetTicketTypesByEventAsync(int eventId);

        // Get a single ticket type by ID
        Task<TicketType> GetTicketTypeByIdAsync(int ticketTypeId);

        // Save (insert/update) a ticket type and return the TicketTypeId
        Task<int> SaveTicketTypeAsync(TicketType ticketType);

        // Delete a ticket type by ID
        Task DeleteTicketTypeAsync(int ticketTypeId);
    }
}
