using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static EventManager.Application.DTOs.ScanDtos;

namespace EventManager.Application.Services
{
    public class ScanService : IScanService
    {
        private readonly IScanRepository _repository;

        public ScanService(IScanRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<object>> ScanLogDetailsAsync(int eventId, int accesspointid)
        {
            try
            {
                return await _repository.GetScanLogAsync(eventId, accesspointid);
            }
            catch (Exception ex)
            {
                return new List<object>();
            }
        }

        public async Task<ScanStatisticsDto> GetScanStatisticsAsync(int eventId, int accesspointid)
        {
            try
            {
                return await _repository.GetScanStatisticsAsync(eventId, accesspointid);
            }
            catch (Exception ex)
            {
                // Handle exception
                return new ScanStatisticsDto();
            }
        }
        public async Task<ScanResultDto> ProcessScanAsync(int eventId, ScanRequestDto request, bool isPrintCenter = false)
        {
            try
            {
                if (!int.TryParse(request.AccessPoint, out int accessPointId))
                    return new ScanResultDto
                    {
                        Success = false,
                        Status = "INVALID",
                        Message = "Invalid access point",
                        ScanTime = DateTime.UtcNow
                    };

                // Hardcoded user ID for now
                int scannedByUserId = 1;

                // Get QR details from stored procedure
                var participant = await _repository.GetQRDetailsAsync(
                    eventId,
                    request.QrCode,
                    accessPointId,
                    scannedByUserId
                );

                // Check if the stored procedure returned a result
                if (participant == null)
                {
                    return new ScanResultDto
                    {
                        Success = false,
                        Status = "ERROR",
                        Message = "Database error occurred",
                        ScanTime = DateTime.UtcNow
                    };
                }

                // Get validation status from stored procedure result
                string validationStatus = participant.ValidationStatus?.ToUpper() ?? "UNKNOWN";
                string validationMessage = participant.ValidationMessage ?? "No validation message";

                // IMPORTANT: Check if scan is valid based on validation status
                bool isScanValid = validationStatus == "VALID";  // Only "VALID" means success

                // Generate ID card only for valid scans in print center
                string idCardHtml = null;
                if (isScanValid && isPrintCenter)
                {
                    // Get pass configuration for ID card template
                    var passConfig = await _repository.GetPassConfigurationAsync(eventId);

                    if (passConfig != null && !string.IsNullOrEmpty(passConfig.BodyText))
                    {
                        // Generate QR code
                        var qrCodeBase64 = GenerateQRCode(request.QrCode, eventId);

                        // Replace placeholders in the HTML template
                        idCardHtml = ReplaceIdCardPlaceholders(
                            passConfig.BodyText,
                            participant,
                            qrCodeBase64
                        );
                    }
                }

                return new ScanResultDto
                {
                    Success = isScanValid,  // TRUE only when validationStatus == "VALID"
                    Status = validationStatus,  // "VALID", "INVALID", "INVALID_ACCESS", "DUPLICATE"
                    Message = validationMessage,  // Message from stored procedure
                    TicketId = participant.ParticipantCode,
                    HolderName = participant.FullName,
                    ScanTime = DateTime.UtcNow,
                    AccessPoint = request.AccessPoint,
                    ParticipantId = participant.ParticipantId,
                    IsPrintCenter = isPrintCenter,
                    IdCardHtml = isScanValid ? idCardHtml : null,
                    ValidationStatus = validationStatus,  // Add these
                    ValidationMessage = validationMessage
                };
            }
            catch (Exception ex)
            {
                return new ScanResultDto
                {
                    Success = false,
                    Status = "ERROR",
                    Message = ex.Message,
                    ScanTime = DateTime.UtcNow
                };
            }
        }
        private string ReplaceIdCardPlaceholders(string template, dynamic participant, string qrCodeBase64 = null)
        {
            if (string.IsNullOrEmpty(template))
                return "<div>No ID card template available</div>";

            var html = template
                .Replace("@EVENTNAME@", participant.EventName?.ToString() ?? "")
                .Replace("@ParticipantName@", participant.FullName?.ToString() ?? "")
                .Replace("@Company@", participant.Company?.ToString() ?? "")
                .Replace("@Department@", participant.Department?.ToString() ?? "")
                .Replace("@ParticipantCode@", participant.ParticipantCode?.ToString() ?? "")
                .Replace("@Email@", participant.Email?.ToString() ?? "")
                .Replace("@EventDate@", participant.EventDate?.ToString() ?? "");

            // IMPORTANT: Replace @QR_BASE64@ with JUST the base64 string, not the whole img tag
            if (!string.IsNullOrEmpty(qrCodeBase64))
            {
                html = html.Replace("@QR_BASE64@", qrCodeBase64);
            }
            else
            {
                // If no QR code, use empty string
                html = html.Replace("@QR_BASE64@", "");
            }

            return html;
        }

        private string GenerateQRCode(string participantCode, int eventId)
        {
            try
            {
                var qrData = $"EVENT:{eventId}|CODE:{participantCode}";
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new Base64QRCode(qrCodeData);
                var qrCodeImageBase64 = qrCode.GetGraphic(20);
                return $"data:image/png;base64,{qrCodeImageBase64}";
            }
            catch
            {
                return null;
            }
        }

       

        //public async Task<List<ScanDtos.ScanLogDto>> GetRecentScansAsync(int eventId)
        //{
        //    return await _repository.GetRecentScansAsync(eventId);
        //}

        //public async Task<ScanDtos.ScanStatsDto> GetScanStatisticsAsync(int eventId)
        //{
        //    return await _repository.GetScanStatisticsAsync(eventId);
        //}
    }
}