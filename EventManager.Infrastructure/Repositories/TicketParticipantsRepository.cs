using Dapper;
using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Data;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class TicketParticipantsRepository : ITicketParticipantsRepository
{
    private readonly DapperContext _context;

    public TicketParticipantsRepository(DapperContext context)
    {
        _context = context;
    }
 
    public async Task<List<ParticipantAssignmentDto>> GetParticipantsForTicketTypeAsync(int ticketTypeId, int eventId)
    {
        using var connection = _context.CreateConnection();
        return (await connection.QueryAsync<ParticipantAssignmentDto>(
            "usp_GetParticipantsForTicketType",
            new { p_TicketTypeId = ticketTypeId, p_EventId = eventId },
            commandType: CommandType.StoredProcedure)).ToList();
    }

    public async Task SaveParticipantAssignmentsAsync(int ticketTypeId, List<int> participantIds, int userId)
    {
        using var connection = _context.CreateConnection();

        string ids = participantIds != null && participantIds.Any()
            ? string.Join(",", participantIds)
            : "";

        await connection.ExecuteAsync(
            "usp_SaveTicketParticipants",
            new
            {
                p_TicketTypeId = ticketTypeId,
                p_ParticipantIds = ids,
                p_UserId = userId
            },
            commandType: CommandType.StoredProcedure);
    }
}