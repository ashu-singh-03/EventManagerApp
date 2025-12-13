using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;


public class TicketParticipantsService : ITicketParticipantsService
{
    private readonly ITicketParticipantsRepository _repository;

    public TicketParticipantsService(ITicketParticipantsRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ParticipantAssignmentDto>> GetParticipantsForTicketTypeAsync(int ticketTypeId, int eventId)
    {
        return await _repository.GetParticipantsForTicketTypeAsync(ticketTypeId, eventId);
    }

    public async Task SaveParticipantAssignmentsAsync(int ticketTypeId, List<int> participantIds, int userId)
    {
        await _repository.SaveParticipantAssignmentsAsync(ticketTypeId, participantIds, userId);
    }
}