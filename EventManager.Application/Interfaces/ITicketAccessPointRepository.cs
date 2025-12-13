using EventManager.Application.DTOs;
using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface ITicketAccessPointRepository
    {
        Task<List<TicketTypeDto>> GetTicketTypesByEventAsync(int eventId);
        Task<List<TicketAccessPointAssignmentDto>> GetAccessPointsForTicketTypeAsync(int ticketTypeId, int eventId);
        Task SaveAssignmentsAsync(int ticketTypeId, List<int> accessPointIds, int userId);
    }
}
