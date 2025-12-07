using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Services
{
    public class AccessPointService : IAccessPointService
    {
        private readonly IAccessPointRepository _repository;

        public AccessPointService(IAccessPointRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AccessPointDto>> GetAccessPointsByEventAsync(int? eventId)
        {
            var accessPoints = await _repository.GetAccessPointsByEventAsync(eventId);
            return accessPoints.Select(ap => new AccessPointDto
            {
                AccessPointId = ap.AccessPointId,
                EventId = ap.EventId,
                Name = ap.Name,
                Description = ap.Description,
                Active = ap.Active
            }).ToList();
        }

        public async Task<AccessPointDto> GetAccessPointByIdAsync(int accessPointId)
        {
            var accessPoint = await _repository.GetAccessPointByIdAsync(accessPointId);
            if (accessPoint == null) return null;

            return new AccessPointDto
            {
                AccessPointId = accessPoint.AccessPointId,
                EventId = accessPoint.EventId,
                Name = accessPoint.Name,
                Description = accessPoint.Description,
                Active = accessPoint.Active
            };
        }

        public async Task SaveAccessPointAsync(AccessPointDto dto)
        {
            var accessPoint = new AccessPoint
            {
                AccessPointId = dto.AccessPointId,
                EventId = dto.EventId,
                Name = dto.Name,
                Description = dto.Description,
                Active = dto.Active,
                UpdatedAt = DateTime.Now
            };

            await _repository.SaveAccessPointAsync(accessPoint);
        }

        public async Task DeleteAccessPointAsync(int accessPointId)
        {
            await _repository.DeleteAccessPointAsync(accessPointId);
        }
    
    }
}
