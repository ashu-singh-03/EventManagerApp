using Dapper;
using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Data;
using System.Data;

namespace EventManager.Infrastructure.Repositories
{
    public class ParticipantCommunicationRepository : IParticipantCommunicationRepository
    {
        private readonly DapperContext _context;

        public ParticipantCommunicationRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<ParticipantCommunicationDto>> GetParticipantsWithAssignmentsAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<ParticipantCommunicationDto>(
                "USP_GetEventParticipantsWithAssignments",
                new { p_event_id = eventId },
                commandType: CommandType.StoredProcedure)).AsList();
        }

        public async Task<dynamic> GetEmailConfigurationAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<dynamic>(
                "USP_GetEmailConfigurationByEventId",
                new { p_event_id = eventId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<dynamic> GetParticipantEmailDataAsync(int eventId, int participantId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<dynamic>(
                "USP_GetParticipantEmailData",
                new
                {
                    p_event_id = eventId,
                    p_participant_id = participantId
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}