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

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Event>(
                "sp_GetAllEvents",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<Event> GetEventByIdAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new { p_event_id = eventId };
            return await connection.QueryFirstOrDefaultAsync<Event>(
                "sp_GetEventById",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task SaveEventAsync(Event evt)
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                p_event_id = evt.EventId,
                p_event_name = evt.EventName,
                p_event_description = evt.EventDescription,
                p_event_date = evt.EventDate,
                p_event_time = evt.EventTime,
                p_location = evt.Location,
                p_end_date = evt.EndDate,
                p_end_time = evt.EndTime,
                p_created_by = evt.CreatedBy ?? "1",
                p_updated_by = evt.UpdatedBy ?? "1"
            };

            await connection.ExecuteAsync(
                "sp_SaveEvent",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task DeleteEventAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new { p_event_id = eventId };
            await connection.ExecuteAsync(
                "sp_DeleteEvent",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}
