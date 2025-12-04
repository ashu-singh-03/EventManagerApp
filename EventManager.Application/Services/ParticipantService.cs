using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Services
{
    public class ParticipantService : IParticipantService
    {
        private readonly IParticipantRepository _repository;

        public ParticipantService(IParticipantRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ParticipantDto>> GetParticipantsByEventAsync(int eventId)
        {
            var participants = await _repository.GetParticipantsByEventAsync(eventId);
            return participants.Select(p => new ParticipantDto
            {
                ParticipantId = p.ParticipantId,
                EventId = p.EventId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                Phone = p.Phone
            });
        }

        public async Task<ParticipantDto> GetParticipantByIdAsync(int participantId)
        {
            var participant = await _repository.GetParticipantByIdAsync(participantId);
            if (participant == null) return null;

            return new ParticipantDto
            {
                ParticipantId = participant.ParticipantId,
                EventId = participant.EventId,
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                Email = participant.Email,
                Phone = participant.Phone
            };
        }

        public async Task SaveParticipantAsync(ParticipantDto dto)
        {
            var participant = new Participant
            {
                ParticipantId = dto.ParticipantId,
                EventId = dto.EventId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                QrCodeHash = Guid.NewGuid().ToString() 
            };

            await _repository.SaveParticipantAsync(participant);
        }

        public async Task DeleteParticipantAsync(int participantId)
        {
            await _repository.DeleteParticipantAsync(participantId);
        }
    }
}
