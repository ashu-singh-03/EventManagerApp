using Dapper;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using EventManager.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Infrastructure.Repositories
{
    public class AccessPointRepository : IAccessPointRepository
    {
        private readonly DapperContext _context;

        public AccessPointRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AccessPoint>> GetAccessPointsByEventAsync(int? eventId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<AccessPoint>(
                "usp_GetAccessPointsByEvent",
                new { p_EventId = eventId },
                commandType: System.Data.CommandType.StoredProcedure);
            return result;
        }

        public async Task<AccessPoint> GetAccessPointByIdAsync(int accessPointId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<AccessPoint>(
                "usp_GetAccessPointById",
                new { p_AccessPointId = accessPointId },
                commandType: System.Data.CommandType.StoredProcedure);
            return result;
        }

        public async Task SaveAccessPointAsync(AccessPoint accessPoint)
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                p_AccessPointId = accessPoint.AccessPointId,
                p_EventId = accessPoint.EventId,
                p_Name = accessPoint.Name,
                p_Description = accessPoint.Description,
                p_CreatedBy = accessPoint.CreatedBy,
                p_UpdatedBy = accessPoint.UpdatedBy,
                p_Active = accessPoint.Active
            };

            await connection.ExecuteAsync(
                "usp_SaveAccessPoint",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeleteAccessPointAsync(int accessPointId)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "usp_DeleteAccessPoint",
                new { p_AccessPointId = accessPointId },
                commandType: System.Data.CommandType.StoredProcedure);
        }
     
    }
}
