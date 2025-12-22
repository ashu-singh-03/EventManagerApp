using Dapper;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EventManager.Infrastructure.Repositories
{
    public  class QrConfigurationRepository : IScanRepository
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
                    p_participants_code = participantsCode, // Changed from p_participant_id
                    p_access_point_id = accessPointId,
                    p_scanned_by = scannedByUserId
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
