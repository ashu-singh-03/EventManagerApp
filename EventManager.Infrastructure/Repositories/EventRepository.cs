using Dapper;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using EventManager.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly DapperContext _context;

        public EventRepository(DapperContext context)
        {
            _context = context;
        }

        // Get all events
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            using var connection = _context.CreateConnection();

            // Dapper will map columns using aliases from stored procedure
            return await connection.QueryAsync<Event>(
                "sp_GetAllEvents",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        // Add new event
        public async Task AddEventAsync(Event newEvent)
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                p_event_name = newEvent.EventName,
                p_event_description = newEvent.EventDescription,
                p_event_date = newEvent.EventDate,
                p_event_time = newEvent.EventTime,
                p_location = newEvent.Location,
                p_end_date = newEvent.EndDate,
                p_end_time = newEvent.EndTime,
                p_created_by = newEvent.CreatedBy,
                p_created_at = newEvent.CreatedAt
            };

            await connection.ExecuteAsync(
                "sp_AddEvent",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
        public async Task DeleteEventAsync(int eventId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new { p_event_id = eventId };

            // Call the stored procedure to soft delete
            await connection.ExecuteAsync(
                "sp_DeleteEvent",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

    }
}
