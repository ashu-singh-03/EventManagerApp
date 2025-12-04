using EventManager.Application.DTOs;
using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IParticipantService
    {
        Task<IEnumerable<ParticipantDto>> GetParticipantsByEventAsync(int eventId);
        Task<ParticipantDto> GetParticipantByIdAsync(int participantId);
        Task SaveParticipantAsync(ParticipantDto participantDto); // insert or update
        Task DeleteParticipantAsync(int participantId);
    }
}
