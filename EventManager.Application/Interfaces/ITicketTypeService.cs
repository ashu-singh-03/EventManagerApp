using EventManager.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Application.Interfaces
{
    public interface ITicketTypeService
    {
        Task<IEnumerable<TicketTypeDto>> GetTicketTypesByEventAsync(int eventId);
        Task<TicketTypeDto> GetTicketTypeByIdAsync(int ticketTypeId);
        Task SaveTicketTypeAsync(TicketTypeDto ticketTypeDto);
        Task DeleteTicketTypeAsync(int ticketTypeId);
    }
}
