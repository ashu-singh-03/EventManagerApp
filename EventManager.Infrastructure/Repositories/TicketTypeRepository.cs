using Dapper;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using EventManager.Infrastructure.Data;
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

        // Get all ticket types for an event
        public async Task<List<TicketType>> GetTicketTypesByEventAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<TicketType>(
                "usp_GetTicketTypesByEvent",
                new { p_EventId = eventId },
                commandType: System.Data.CommandType.StoredProcedure
            );

            return result.AsList(); // Convert IEnumerable<T> to List<T>
        }

        // Get a single ticket type by ID
        public async Task<TicketType> GetTicketTypeByIdAsync(int ticketTypeId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<TicketType>(
                "sp_GetTicketTypeById",
                new { p_TicketTypeId = ticketTypeId },
                commandType: System.Data.CommandType.StoredProcedure
            );

            return result;
        }

        // Save ticket type (insert or update) and return the TicketTypeId
        public async Task<int> SaveTicketTypeAsync(TicketType ticketType)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_TicketTypeId", ticketType.TicketTypeId, System.Data.DbType.Int32, System.Data.ParameterDirection.InputOutput);
            parameters.Add("p_EventId", ticketType.EventId);
            parameters.Add("p_TicketName", ticketType.TicketName);
            parameters.Add("p_Price", ticketType.Price);
            parameters.Add("p_BookingTypeID", ticketType.BookingTypeID);
            parameters.Add("p_IsCapacityUnlimited", ticketType.IsCapacityUnlimited);
            parameters.Add("p_MinCapacity", ticketType.MinCapacity);
            parameters.Add("p_MaxCapacity", ticketType.MaxCapacity);
            parameters.Add("p_SalesEndDate", ticketType.SalesEndDate);
            parameters.Add("p_Description", ticketType.Description);
            parameters.Add("p_IsFreeTicket", ticketType.IsFreeTicket);
            parameters.Add("p_CreatedBy", ticketType.CreatedBy);
            parameters.Add("p_ModifiedBy", ticketType.ModifiedBy);

            await connection.ExecuteAsync(
                "usp_SaveTicketType",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );

            // Return the ID of the saved ticket
            return parameters.Get<int>("p_TicketTypeId");
        }

        // Delete ticket type
        public async Task DeleteTicketTypeAsync(int ticketTypeId)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "usp_DeleteTicketType",
                new { p_TicketTypeId = ticketTypeId },
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}
