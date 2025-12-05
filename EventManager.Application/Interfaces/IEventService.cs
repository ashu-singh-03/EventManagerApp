using EventManager.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Application.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<EventDto>> GetAllEventsAsync();
        Task<EventDto> GetEventByIdAsync(int eventId);
        Task<EventWithTicketsDto> GetEventWithTicketsByIdAsync(int eventId);
        Task<int> SaveEventAsync(EventDto dto);
        Task DeleteEventAsync(int eventId);
    }
}
