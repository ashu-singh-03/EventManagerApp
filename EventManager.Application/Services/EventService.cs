using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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

        public async Task AddEventAsync(Event newEvent)
        {
            newEvent.CreatedAt = DateTime.Now;
            newEvent.Status = "Active"; // default status
            await _eventRepository.AddEventAsync(newEvent);
        }
        public async Task DeleteEventAsync(int eventId)
        {
            await _eventRepository.DeleteEventAsync(eventId);
        }
    }
}
