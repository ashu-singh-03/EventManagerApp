using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repository;
        private readonly ITicketTypeRepository _ticketRepository;

        public EventService(IEventRepository repository, ITicketTypeRepository ticketRepository)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
        {
            var events = await _repository.GetAllEventsAsync();
            return events.Select(e => new EventDto
            {
                EventId = e.EventId,
                EventName = e.EventName,
                EventDescription = e.EventDescription,
                EventDate = e.EventDate.ToString("yyyy-MM-dd"),
                EventTime = e.EventTime?.ToString(@"hh\:mm"),
                Location = e.Location,
                EndDate = e.EndDate?.ToString("yyyy-MM-dd"),
                EndTime = e.EndTime?.ToString(@"hh\:mm")
            }).ToList();
        }

        public async Task<EventDto> GetEventByIdAsync(int eventId)
        {
            var e = await _repository.GetEventByIdAsync(eventId);
            if (e == null) return null;

            return new EventDto
            {
                EventId = e.EventId,
                EventName = e.EventName,
                EventDescription = e.EventDescription,
                EventDate = e.EventDate.ToString("yyyy-MM-dd"),
                EventTime = e.EventTime?.ToString(@"hh\:mm"),
                Location = e.Location,
                EndDate = e.EndDate?.ToString("yyyy-MM-dd"),
                EndTime = e.EndTime?.ToString(@"hh\:mm")
            };
        }

        public async Task<EventWithTicketsDto> GetEventWithTicketsByIdAsync(int eventId)
        {
            // Get the event
            var e = await _repository.GetEventByIdAsync(eventId);
            if (e == null) return null;

            // Get all tickets for this event
            var tickets = await _ticketRepository.GetTicketTypesByEventAsync(eventId);

            return new EventWithTicketsDto
            {
                Event = new EventDto
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventDescription = e.EventDescription,
                    EventDate = e.EventDate.ToString("yyyy-MM-dd"),
                    EventTime = e.EventTime?.ToString(@"hh\:mm"),
                    Location = e.Location,
                    EndDate = e.EndDate?.ToString("yyyy-MM-dd"),
                    EndTime = e.EndTime?.ToString(@"hh\:mm")
                },
                TicketTypes = tickets.Select(t => new TicketTypeDto
                {
                    TicketTypeId = t.TicketTypeId,
                    EventId = t.EventId,
                    TicketName = t.TicketName,
                    Price = t.Price,
                    BookingTypeID = t.BookingTypeID,
                    IsCapacityUnlimited = t.IsCapacityUnlimited,
                    MinCapacity = t.MinCapacity,
                    MaxCapacity = t.MaxCapacity,
                    SalesEndDate = t.SalesEndDate,
                    Description = t.Description,
                    IsFreeTicket = t.IsFreeTicket
                }).ToList()
            };
        }

        public async Task<int> SaveEventAsync(EventDto dto)
        {
            var evt = new Event
            {
                EventId = dto.EventId,
                EventName = dto.EventName,
                EventDescription = dto.EventDescription,
                EventDate = DateTime.Parse(dto.EventDate),
                EventTime = string.IsNullOrEmpty(dto.EventTime) ? null : TimeSpan.Parse(dto.EventTime),
                Location = dto.Location,
                EndDate = string.IsNullOrEmpty(dto.EndDate) ? null : DateTime.Parse(dto.EndDate),
                EndTime = string.IsNullOrEmpty(dto.EndTime) ? null : TimeSpan.Parse(dto.EndTime),
                IsActive = true
            };

            if (evt.EventId == 0)
            {
                evt.CreatedAt = DateTime.Now;
                evt.CreatedBy ??= "1";
            }
            else
            {
                evt.UpdatedAt = DateTime.Now;
                evt.UpdatedBy ??= "1";
            }

            var savedId = await _repository.SaveEventAsync(evt);
            dto.EventId = savedId;
            return savedId;
        }

        public async Task DeleteEventAsync(int eventId)
        {
            await _repository.DeleteEventAsync(eventId);
        }
    }
}
