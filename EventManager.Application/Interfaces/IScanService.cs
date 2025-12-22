using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using static EventManager.Application.DTOs.ScanDtos;

namespace EventManager.Application.Interfaces
{
    public interface IScanService
    {
        Task<ScanResultDto> ProcessScanAsync(int eventId, ScanRequestDto request, bool isPrintCenter = false);
        Task<List<ScanLogDto>> ScanLogDetailsAsync(int eventId, int accesspointid);
        Task<ScanStatisticsDto> GetScanStatisticsAsync(int eventId, int accesspointid);
    }

}
