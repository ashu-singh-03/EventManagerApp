using Dapper;
using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static EventManager.Application.DTOs.ScanDtos;

namespace EventManager.Infrastructure.Repositories
{
    public class QrConfigurationRepository : IScanRepository
    {
        private readonly DapperContext _context;

        public QrConfigurationRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetPassConfigurationAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<dynamic>(
                "USP_GetPassConfigurationByEventId",
                new { p_event_id = eventId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<dynamic> GetQRDetailsAsync(int eventId, string participantsCode, int accessPointId, int scannedByUserId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<dynamic>(
                "USP_ValidateQRAndGetDetails_V1",
                new
                {
                    p_event_id = eventId,
                    p_participants_code = participantsCode,
                    p_access_point_id = accessPointId,
                    p_scanned_by = scannedByUserId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<ScanLogDto>> GetScanLogAsync(int eventId, int accesspointid)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<ScanLogDto>(
                "USP_GetScanLogByEventAndAccessPoint",
                new
                {
                    p_event_id = eventId,
                    p_access_point_id = accesspointid,
                    p_user_id = (int?)null // Pass null since user_id is optional
                },
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<ScanStatisticsDto> GetScanStatisticsAsync(int eventId, int accesspointid)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<ScanStatisticsDto>(
                "USP_GetScanStatistics",
                new
                {
                    p_event_id = eventId,
                    p_access_point_id = accesspointid,
                    p_user_id = (int?)null // Pass null since user_id is optional
                },
                commandType: CommandType.StoredProcedure);

            return result ?? new ScanStatisticsDto();
        }
    }
}