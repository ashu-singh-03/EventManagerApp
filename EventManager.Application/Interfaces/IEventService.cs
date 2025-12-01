using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task DeleteEventAsync(int eventId);
        Task AddEventAsync(Event newEvent);
    }
}
