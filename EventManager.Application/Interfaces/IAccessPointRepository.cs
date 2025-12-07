using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IAccessPointRepository
    {
        Task<IEnumerable<AccessPoint>> GetAccessPointsByEventAsync(int? eventId);
        Task<AccessPoint> GetAccessPointByIdAsync(int accessPointId);
        Task SaveAccessPointAsync(AccessPoint accessPoint);
        Task DeleteAccessPointAsync(int accessPointId);
    }
}
