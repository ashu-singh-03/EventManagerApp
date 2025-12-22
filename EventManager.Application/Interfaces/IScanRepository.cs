using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using static EventManager.Application.DTOs.ScanDtos;

namespace EventManager.Application.Interfaces
{
    public interface IScanRepository
    {
        Task <dynamic> GetPassConfigurationAsync(int eventId);
        Task<dynamic> GetQRDetailsAsync(int eventId, string participantId, int accesspointid, int scannedByUserId);

    }

}
