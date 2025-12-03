using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _eventRepository.GetAllEventsAsync();
        }

        public async Task<Event> GetEventByIdAsync(int eventId)
        {
            return await _eventRepository.GetEventByIdAsync(eventId);
        }

        public async Task SaveEventAsync(Event evt)
        {
            if (evt.EventId == 0)
                evt.CreatedAt = DateTime.Now;
            else
                evt.UpdatedBy = "1"; // Replace with actual user

            await _eventRepository.SaveEventAsync(evt);
        }

        public async Task DeleteEventAsync(int eventId)
        {
            await _eventRepository.DeleteEventAsync(eventId);
        }
    }
}
