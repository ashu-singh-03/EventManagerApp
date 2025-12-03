using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IParticipantRepository
    {
        Task<IEnumerable<Participant>> GetParticipantsByEventAsync(int eventId);
        Task<Participant> GetParticipantByIdAsync(int participantId);
        Task SaveParticipantAsync(Participant participant); 
        Task DeleteParticipantAsync(int participantId);
    }
}
