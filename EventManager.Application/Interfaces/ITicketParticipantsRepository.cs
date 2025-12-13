using EventManager.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Application.Interfaces
{
    public interface ITicketParticipantsRepository
    {
        Task<List<ParticipantAssignmentDto>> GetParticipantsForTicketTypeAsync(int ticketTypeId, int eventId);
        Task SaveParticipantAssignmentsAsync(int ticketTypeId, List<int> participantIds, int userId);
    }
}