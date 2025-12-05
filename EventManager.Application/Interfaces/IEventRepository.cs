using EventManager.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Application.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event> GetEventByIdAsync(int eventId);
        Task<int> SaveEventAsync(Event evt);
        Task DeleteEventAsync(int eventId);
    }
}
