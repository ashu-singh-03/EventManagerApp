using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IParticipantCommunicationService
    {
        Task<List<ParticipantCommunicationDto>> GetParticipantsWithAssignmentsAsync(int eventId);
        Task<EmailResponse> SendEmailToParticipantAsync(int eventId, int participantId);

    }
}
