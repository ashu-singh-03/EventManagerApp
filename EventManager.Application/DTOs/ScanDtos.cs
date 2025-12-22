using System;
using System.Collections.Generic;

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
        }

        public class ScanStatisticsDto
        {
            public int TotalScans { get; set; }
            public int ValidScans { get; set; }
            public int InvalidScans { get; set; }
            public int DuplicateScans { get; set; }
        }

        public class ScanLogDto
        {
            // Keep only properties you want to show
            public string EventName { get; set; }
            public DateTime? EventDate { get; set; }
            //public string TicketId { get; set; }
            public string ParticipantName { get; set; }
            public string AccessPoint { get; set; }
            public DateTime ScanTime { get; set; }
            public string ValidationStatus { get; set; }
            public string ValidationMessage { get; set; }
            //public int? ScannedBy { get; set; }
            //public bool? IsPrintCenter { get; set; }

            // Remove these:
            // public int ScanId { get; set; }
            // public int EventId { get; set; }
        }
    }
}