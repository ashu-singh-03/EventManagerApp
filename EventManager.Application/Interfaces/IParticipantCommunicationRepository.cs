using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IParticipantCommunicationRepository
    {
        Task<List<ParticipantCommunicationDto>> GetParticipantsWithAssignmentsAsync(int eventId);
        Task<dynamic> GetEmailConfigurationAsync(int eventId);
        Task<dynamic> GetParticipantEmailDataAsync(int eventId, int participantId);
    }
}
