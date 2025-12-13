using Dapper;
using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Data;
using System.Data;

public class TicketAccessPointRepository : ITicketAccessPointRepository
{
    private readonly DapperContext _context;

    public TicketAccessPointRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<List<TicketTypeDto>> GetTicketTypesByEventAsync(int eventId)
    {
        using var connection = _context.CreateConnection();
        return (await connection.QueryAsync<TicketTypeDto>(
            "usp_GetTicketTypesByEvent",
            new { p_EventId = eventId },
            commandType: CommandType.StoredProcedure)).ToList();
    }

    public async Task<List<TicketAccessPointAssignmentDto>> GetAccessPointsForTicketTypeAsync(int ticketTypeId, int eventId)
    {
        using var connection = _context.CreateConnection();
        return (await connection.QueryAsync<TicketAccessPointAssignmentDto>(
            "usp_GetAccessPointsForTicketType",
            new { p_TicketTypeId = ticketTypeId, p_EventId = eventId },
            commandType: CommandType.StoredProcedure)).ToList();
    }

    public async Task SaveAssignmentsAsync(int ticketTypeId, List<int> accessPointIds, int userId)
    {
        using var connection = _context.CreateConnection();

        string ids = accessPointIds != null && accessPointIds.Any()
            ? string.Join(",", accessPointIds)
            : "";

        await connection.ExecuteAsync(
            "usp_SaveTicketAccessPoints",
            new
            {
                p_TicketTypeId = ticketTypeId,
                p_AccessPointIds = ids,
                p_UserId = userId
            },
            commandType: CommandType.StoredProcedure);
    }
}