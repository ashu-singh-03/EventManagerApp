using Dapper;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using EventManager.Infrastructure.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Infrastructure.Repositories
{
    public class ParticipantRepository : IParticipantRepository
    {
        private readonly DapperContext _context;

        public ParticipantRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Participant>> GetParticipantsByEventAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<Participant>(
                "sp_GetParticipantsByEvent",
                new { p_EventId = eventId }, // must match SP parameter exactly
                commandType: System.Data.CommandType.StoredProcedure);
            return result;
        }


        public async Task<Participant> GetParticipantByIdAsync(int participantId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<Participant>(
                "sp_GetParticipantById",
                new { p_ParticipantId = participantId }, // Must match SP parameter exactly
                commandType: System.Data.CommandType.StoredProcedure);
            return result;
        }


        public async Task SaveParticipantAsync(Participant participant)
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                p_ParticipantId = participant.ParticipantId,
                p_EventId = participant.EventId,
                p_FirstName = participant.FirstName,
                p_LastName = participant.LastName,
                p_Email = participant.Email,
                p_Phone = participant.Phone,
                p_Company = participant.Company,         
                p_Department = participant.Department,  
                p_Notes = participant.Notes,
                p_QrCodeHash = participant.QrCodeHash,
                p_CreatedBy = participant.CreatedBy,
                p_UpdatedBy = participant.UpdatedBy
            };

            await connection.ExecuteAsync(
                "sp_SaveParticipant",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);
        }


        public async Task DeleteParticipantAsync(int participantId)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "sp_DeleteParticipant",
                new { p_ParticipantId = participantId }, // must match SP parameter name
                commandType: System.Data.CommandType.StoredProcedure);
        }

    }
}
