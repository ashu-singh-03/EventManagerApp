using EventManager.Application.DTOs;
using EventManager.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IParticipantService
    {
        Task<IEnumerable<ParticipantDto>> GetParticipantsByEventAsync(int eventId);
        Task<ParticipantDto> GetParticipantByIdAsync(int participantId);
        Task SaveParticipantAsync(ParticipantDto participantDto);
        Task DeleteParticipantAsync(int participantId);
        Task<ImportResult> ImportParticipantsFromExcelAsync(IFormFile excelFile, int eventId, string createdBy);
    }
}
