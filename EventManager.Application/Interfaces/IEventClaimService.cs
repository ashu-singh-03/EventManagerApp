using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IEventClaimService
    {
        Task SetEventIdClaimAsync(int eventId);
        int GetEventIdFromClaim();
    }
}
