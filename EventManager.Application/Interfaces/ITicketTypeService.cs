using EventManager.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Application.Interfaces
{
    public interface ITicketTypeService
    {
        Task<List<TicketTypeDto>> GetTicketTypesByEventAsync(int eventId); // list for multiple tickets
        Task<TicketTypeDto> GetTicketTypeByIdAsync(int ticketTypeId);
        Task<int> SaveTicketTypeAsync(TicketTypeDto ticketTypeDto); // return saved ticket id
        Task DeleteTicketTypeAsync(int ticketTypeId);
    }
}
