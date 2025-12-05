using Dapper;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using EventManager.Infrastructure.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Infrastructure.Repositories
{
    public class TicketTypeRepository : ITicketTypeRepository
    {
        private readonly DapperContext _context;

        public TicketTypeRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TicketType>> GetTicketTypesByEventAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<TicketType>(
                "sp_GetTicketTypesByEvent",
                new { p_EventId = eventId },
                commandType: System.Data.CommandType.StoredProcedure);

            return result;
        }

        public async Task<TicketType> GetTicketTypeByIdAsync(int ticketTypeId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<TicketType>(
                "sp_GetTicketTypeById",
                new { p_TicketTypeId = ticketTypeId },
                commandType: System.Data.CommandType.StoredProcedure);

            return result;
        }

        public async Task SaveTicketTypeAsync(TicketType ticketType)
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                p_TicketTypeId = ticketType.TicketTypeId,
                p_EventId = ticketType.EventId,
                p_TicketName = ticketType.TicketName,
                p_Price = ticketType.Price,
                p_BookingTypeID = ticketType.BookingTypeID,
                p_IsCapacityUnlimited = ticketType.IsCapacityUnlimited,
                p_MinCapacity = ticketType.MinCapacity,
                p_MaxCapacity = ticketType.MaxCapacity,
                p_SalesEndDate = ticketType.SalesEndDate,
                p_Description = ticketType.Description,
                p_IsFreeTicket = ticketType.IsFreeTicket,
                p_CreatedBy = ticketType.CreatedBy,
                p_ModifiedBy = ticketType.ModifiedBy
            };

            await connection.ExecuteAsync(
                "usp_SaveTicketType",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);
        }
        public async Task DeleteTicketTypeAsync(int ticketTypeId)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "sp_DeleteTicketType",
                new { p_TicketTypeId = ticketTypeId },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
