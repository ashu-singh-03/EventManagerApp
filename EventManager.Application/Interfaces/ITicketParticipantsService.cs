using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface ITicketParticipantsService
    {
        Task<List<ParticipantAssignmentDto>> GetParticipantsForTicketTypeAsync(int ticketTypeId, int eventId);
        Task SaveParticipantAssignmentsAsync(int ticketTypeId, List<int> participantIds, int userId);
    }
}
