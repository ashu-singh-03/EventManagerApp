using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task AddEventAsync(Event newEvent);
        Task DeleteEventAsync(int eventId);
    }
}
