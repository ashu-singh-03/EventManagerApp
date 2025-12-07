using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IAccessPointService
    {
        Task<IEnumerable<AccessPointDto>> GetAccessPointsByEventAsync(int? eventId);
        Task<AccessPointDto> GetAccessPointByIdAsync(int accessPointId);
        Task SaveAccessPointAsync(AccessPointDto accessPointDto);
        Task DeleteAccessPointAsync(int accessPointId);
    }
}
