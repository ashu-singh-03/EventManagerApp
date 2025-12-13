using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;

public class TicketAccessPointService : ITicketAccessPointService
{
    private readonly ITicketAccessPointRepository _repository;

    public TicketAccessPointService(ITicketAccessPointRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TicketTypeDto>> GetTicketTypesAsync(int eventId)
    {
        return await _repository.GetTicketTypesByEventAsync(eventId);
    }

    public async Task<List<TicketAccessPointAssignmentDto>> GetAccessPointsForTicketTypeAsync(int ticketTypeId, int eventId)
    {
        return await _repository.GetAccessPointsForTicketTypeAsync(ticketTypeId, eventId);
    }

    public async Task SaveAssignmentsAsync(int ticketTypeId, List<int> accessPointIds, int userId)
    {
        await _repository.SaveAssignmentsAsync(ticketTypeId, accessPointIds, userId);
    }
}