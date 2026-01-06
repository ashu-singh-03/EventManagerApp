using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using iText.Barcodes;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
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
        public async Task<ScanResultDto> ProcessScanAsync(int eventId, ScanRequestDto request, bool isPrintCenter = false, bool isReprint = false)
        {
            try
            {
                var arreventId = request.QrCode.Replace("/", "||").Split("||");
                if (arreventId.Length > 1)
                {
                    int value = Convert.ToInt32(arreventId[1]);
                    if (value > 0)
                    {
                        eventId = Convert.ToInt32(arreventId[1]);
                    }
                }
                var participantId = arreventId[0];

                if (!int.TryParse(request.AccessPoint, out int accessPointId))
                    return new ScanResultDto
                    {
                        Success = false,
                        Status = "INVALID",
                        Message = "Invalid access point",
                        ScanTime = DateTime.UtcNow
                    };

                int scannedByUserId = 1;

                // Get QR details from stored procedure WITH isReprint parameter
                var participant = await _repository.GetQRDetailsAsync(
                    eventId,
                    participantId,
                    accessPointId,
                    scannedByUserId,
                    isReprint  // PASS isReprint parameter
                );

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

                string validationStatus = participant.ValidationStatus?.ToUpper() ?? "UNKNOWN";
                string validationMessage = participant.ValidationMessage ?? "No validation message";
                bool isScanValid = validationStatus == "VALID";

                byte[] pdfBytes = null;
                string qrCodeBase64 = null;

                if (isPrintCenter)
                {
                    // Generate QR code image as base64 for frontend display
                    qrCodeBase64 = GenerateQRCode(participantId, eventId);

                    // Create QR data string for PDF generation
                    var qrData = $"{participantId}||{eventId}";

                    // Generate ID card PDF
                    pdfBytes = await GenerateIDCard(participant, qrData);
                }

                return new ScanResultDto
                {
                    Success = isScanValid,
                    Status = validationStatus,
                    Message = validationMessage,
                    TicketId = participant.ParticipantCode,
                    QrCodeBase64 = qrCodeBase64, // ADDED: QR image for frontend
                    HolderName = participant.FullName,
                    FullName = participant.FullName,
                    ParticipantCode = participant.ParticipantCode,
                    Company = participant.Company,
                    Country = participant.Country,
                    ScanTime = DateTime.UtcNow,
                    AccessPoint = request.AccessPoint,
                    ParticipantId = participant.ParticipantId,
                    IsPrintCenter = isPrintCenter,
                    pdfBytes = pdfBytes,
                    ValidationStatus = validationStatus,
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
        public async Task<byte[]> GenerateIDCard(dynamic participantData, string qrCodeValue)
        {
            double pointsPerMm = 72d / 25.4d;
            PageSize a6Page = PageSize.A6;

            float contentWidth = (float)(95f * pointsPerMm);
            float contentHeight = (float)(53f * pointsPerMm);
            float qrSize = (float)(20f * pointsPerMm);

            // Position of the main box
            float startY = (float)(25f * pointsPerMm);
            float startX = (a6Page.GetWidth() - contentWidth) / 2;

            PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using (MemoryStream pdfStream = new MemoryStream())
            {
                using (PdfWriter pdfWriter = new PdfWriter(pdfStream))
                using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
                {
                    pdfDocument.SetDefaultPageSize(a6Page);
                    Document document = new Document(pdfDocument);
                    document.SetMargins(0, 0, 0, 0);

                    // --- 1. THE MAIN CONTENT BOX ---
                    Div container = new Div()
                        .SetWidth(contentWidth)
                        .SetHeight(contentHeight)
                        .SetFixedPosition(startX, startY, contentWidth)
                        .SetBorder(new SolidBorder(ColorConstants.BLACK, 1f));

                    Table layoutTable = new Table(1).SetWidth(contentWidth).SetHeight(contentHeight);

                    // Top Section: Name & Company
                    Div topText = new Div().SetTextAlignment(TextAlignment.CENTER).SetPaddingTop(5);
                    topText.Add(new Paragraph(participantData.FullName ?? "N/A")
                        .SetFont(fontBold).SetFontSize(23).SetMargin(0).SetMultipliedLeading(1.0f));
                    topText.Add(new Paragraph(participantData.Company ?? "")
                        .SetFont(fontBold).SetFontSize(15).SetMargin(0));

                    layoutTable.AddCell(new Cell().Add(topText).SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.TOP));

                    // Bottom Section: Country & QR Code
                    Div bottomArea = new Div().SetTextAlignment(TextAlignment.CENTER);
                    bottomArea.Add(new Paragraph(participantData.Country ?? "")
                        .SetFont(fontRegular).SetFontSize(13).SetMarginBottom(2));

                    // Generate QR Code Object
                    BarcodeQRCode qrCode = new BarcodeQRCode(qrCodeValue ?? "Empty");
                    Image qrImage = new Image(qrCode.CreateFormXObject(pdfDocument))
                        .SetWidth(qrSize).SetHeight(qrSize)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    bottomArea.Add(qrImage);

                    layoutTable.AddCell(new Cell().Add(bottomArea).SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.BOTTOM).SetPaddingBottom(2));

                    container.Add(layoutTable);
                    document.Add(container);
                    document.Close();
                }
                return pdfStream.ToArray();
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
                .Replace("@EventDate@", participant.EventDate?.ToString() ?? "")
                .Replace("@Country@", participant.Country?.ToString() ?? "");
                

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
                //var qrData = $"EVENT:{eventId}|CODE:{participantCode}";
                //using var qrGenerator = new QRCodeGenerator();
                //var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                //var qrCode = new Base64QRCode(qrCodeData);
                //var qrCodeImageBase64 = qrCode.GetGraphic(20);
                //return $"data:image/png;base64,{qrCodeImageBase64}";


                var qrData = participantCode + "||" + eventId;

                // 1. Use raw text WITHOUT any prefix


                // 2. Generate QR code
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);

                // 3. Use PngByteQRCode (most reliable)
                var pngQrCode = new PngByteQRCode(qrCodeData);

                // 4. Generate with iPhone-friendly settings
                var pngBytes = pngQrCode.GetGraphic(
                    pixelsPerModule: 10,  // Optimal for iPhone
                    drawQuietZones: true //, // CRITICAL for iPhone
                                         //  quietZoneRendering: QRCoder.QRCodeGenerator.QuietZoneRendering.Flat
                );

                // 5. Convert to base64
                var base64 = Convert.ToBase64String(pngBytes);

                // 6. Test the QR code before returning
                //   TestQRCode(pngBytes, qrData);

                return $"data:image/png;base64,{base64}";
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