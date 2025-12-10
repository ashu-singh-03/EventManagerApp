using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Application.Services
{
    public class TicketTypeService : ITicketTypeService
    {
        private readonly ITicketTypeRepository _repository;

        public TicketTypeService(ITicketTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TicketTypeDto>> GetTicketTypesByEventAsync(int eventId)
        {
            var items = await _repository.GetTicketTypesByEventAsync(eventId);
            return items.Select(t => new TicketTypeDto
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
            }).ToList();
        }

        public async Task<TicketTypeDto> GetTicketTypeByIdAsync(int ticketTypeId)
        {
            var t = await _repository.GetTicketTypeByIdAsync(ticketTypeId);
            if (t == null) return null;

            return new TicketTypeDto
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
            };
        }

        public async Task<int> SaveTicketTypeAsync(TicketTypeDto dto)
        {
            var ticketType = new TicketType
            {
                TicketTypeId = dto.TicketTypeId,
                EventId = dto.EventId,
                TicketName = dto.TicketName,
                Price = dto.Price,
                BookingTypeID = dto.BookingTypeID,
                IsCapacityUnlimited = dto.IsCapacityUnlimited,
                MinCapacity = dto.MinCapacity,
                MaxCapacity = dto.MaxCapacity,
                SalesEndDate = dto.SalesEndDate,
                Description = dto.Description,
                IsFreeTicket = dto.IsFreeTicket,
                CreatedBy = dto.UserId,
                ModifiedBy = dto.UserId
            };

            var savedId = await _repository.SaveTicketTypeAsync(ticketType);
            return savedId;
        }

        public async Task DeleteTicketTypeAsync(int ticketTypeId)
        {
            await _repository.DeleteTicketTypeAsync(ticketTypeId);
        }
    }
}
