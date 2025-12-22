using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class ScanDtos
    {
        public class ScanRequestDto
        {
            public string QrCode { get; set; }
            public string AccessPoint { get; set; }
            public bool IsPrintCenter { get; set; }
        }

        public class ScanResultDto
        {
            public bool Success { get; set; }
            public string TicketId { get; set; }
            public string HolderName { get; set; }
            public string Status { get; set; }
            public DateTime ScanTime { get; set; }
            public string AccessPoint { get; set; }
            public string Message { get; set; }
            public int? ParticipantId { get; set; }
            public bool IsPrintCenter { get; set; }
            public string IdCardHtml { get; set; }

            public string ValidationStatus { get; set; }
            public string ValidationMessage { get; set; }
            public string FullName { get; set; }
            public string ParticipantCode { get; set; }

            // Optional: Add more details from the stored procedure
            public string EventName { get; set; }
            public string EventDate { get; set; }
            public string EventTime { get; set; }
            public string Location { get; set; }
            public string Email { get; set; }
            public string Company { get; set; }
            public string Department { get; set; }
            public string Phone { get; set; }
            public string TicketTypes { get; set; }
            public string AccessPoints { get; set; }
            public bool? AllowMultipleScans { get; set; }
            public DateTime? LastScanTime { get; set; }
        }
        public class ScanStatsDto
        {
            public int TotalScans { get; set; }
            public int ValidScans { get; set; }
            public int InvalidScans { get; set; }
            public int DuplicateScans { get; set; }
        }

        public class ScanLogDto
        {
            public int Id { get; set; }
            public string TicketId { get; set; }
            public string HolderName { get; set; }
            public string ScanTime { get; set; }
            public string Status { get; set; }
            public string AccessPoint { get; set; }
        }
    }
}