using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface ITicketAccessPointService
    {
        Task<List<TicketTypeDto>> GetTicketTypesAsync(int eventId);
        Task<List<TicketAccessPointAssignmentDto>> GetAccessPointsForTicketTypeAsync(int ticketTypeId, int eventId);
        Task SaveAssignmentsAsync(int ticketTypeId, List<int> accessPointIds, int userId);
    }
}
