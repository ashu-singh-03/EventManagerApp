using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IParticipantRepository
    {
        Task<IEnumerable<Participant>> GetParticipantsByEventAsync(int eventId);
        Task<Participant> GetParticipantByIdAsync(int participantId);
        Task SaveParticipantAsync(Participant participant);
        Task DeleteParticipantAsync(int participantId);
        Task DeleteTempParticipantsAsync(int eventId, string createdBy);
        Task BulkInsertToTempTableAsync(DataTable data);
        Task<DataTable> ValidateTempParticipantsAsync(int eventId, string createdBy);
        Task<int> ImportTempToMainAsync(int eventId, string createdBy);
    }
}
