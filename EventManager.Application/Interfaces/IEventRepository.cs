using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event> GetEventByIdAsync(int eventId);

        // Single method for insert/update
        Task SaveEventAsync(Event evt);

        Task DeleteEventAsync(int eventId);
    }
}
