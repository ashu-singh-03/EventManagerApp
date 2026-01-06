using DinkToPdf;
using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Domain.Entities;
using EventManager.WebUI.ViewComponents;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using OfficeOpenXml;
using QuestPDF.Fluent;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO.Compression;
using System.Threading.Tasks;
using static EventManager.Application.DTOs.ScanDtos;

namespace EventManager.WebUI.Controllers
{
    public class ParticipantCommunicationController : Controller
    {
        private readonly IParticipantCommunicationService _service;
        private readonly IEventClaimService _eventClaimService;
        public ParticipantCommunicationController(
            IParticipantCommunicationService service,
            IEventClaimService eventClaimService)
        {
            _service = service;
            _eventClaimService = eventClaimService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadParticipantsWithAssignments()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return Json(new { success = false, message = "Invalid event" });

            var participants = await _service.GetParticipantsWithAssignmentsAsync(eventId);
            return Json(new { success = true, data = participants });
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailToParticipant([FromBody] EmailRequestDto request)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return Json(new { success = false, message = "Invalid event" });

            var result = await _service.SendEmailToParticipantAsync(eventId, request.ParticipantId);

            return Json(new
            {
                success = result.Success,
                message = result.Success ? "Email sent successfully" : result.Error
            });
        }


        [HttpPost]
        public async Task<IActionResult> GenerateIdCard([FromBody] ScanRequestDto request)
        {
            try
            {
                int eventId = _eventClaimService.GetEventIdFromClaim();
                if (eventId == 0) return Json(new { success = false, message = "Invalid event" });

                if (!int.TryParse(request.QrCode, out int participantId))
                {
                    return Json(new { success = false, message = "Invalid QR code format." });
                }

                var result = await _service.GenerateIdCardAsync(eventId, participantId);

                return Json(new
                {
                    success = result.Success,
                    idCardPdfBase64 = result.IdCardPdf != null ? Convert.ToBase64String(result.IdCardPdf) : null,
                    message = result.ValidationMessage,
                    participantId = result.ParticipantId,
                    fullName = result.FullName,
                    participantCode = result.ParticipantCode
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> LogCardAction([FromBody] LogCardActionDto request)
        {
            try
            {
                int eventId = _eventClaimService.GetEventIdFromClaim();
                if (eventId == 0)
                    return Json(new { success = false, message = "Invalid event" });

                int userId = 1; // Get from claim/session
                int actionType = request.IsPrintAction ? 1 : 0;

                // Log the action and get updated count
                int updatedCount = await _service.LogCardActionAsync(eventId, request.ParticipantId, userId, request.IsPrintAction);

                if (updatedCount >= 0)
                {
                    return Json(new
                    {
                        success = true,
                        count = updatedCount,
                        actionType = actionType
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to log action" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateBulkIdCards([FromBody] List<ScanRequestDto> requests)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (requests == null || !requests.Any())
            {
                return Json(new { success = false, message = "No participants selected" });
            }

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        int generatedCount = 0;

                        foreach (var request in requests)
                        {
                            try
                            {
                                if (!int.TryParse(request.QrCode, out int participantId))
                                {
                                    continue;
                                }

                                // Get HTML from service
                                var idCardResult = await _service.GenerateIdCardAsync(eventId, participantId);

                                if (!idCardResult.Success || string.IsNullOrEmpty(idCardResult.IdCardHtml))
                                {
                                    continue;
                                }

                                // Parse HTML to extract data
                                var htmlDoc = new HtmlDocument();
                                htmlDoc.LoadHtml(idCardResult.IdCardHtml);

                                var nameNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='name']");
                                var companyNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='company']");
                                var countryNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='country']");
                                var qrImgNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@class='qr-placeholder']");

                                string name = nameNode?.InnerText?.Trim() ?? request.ParticipantName ?? "Unknown";
                                string company = companyNode?.InnerText?.Trim() ?? "";
                                string country = countryNode?.InnerText?.Trim() ?? "United Kingdom";

                                byte[] qrImage = null;
                                if (qrImgNode != null)
                                {
                                    var src = qrImgNode.GetAttributeValue("src", "");
                                    if (src.StartsWith("data:image/png;base64,"))
                                    {
                                        try
                                        {
                                            var base64String = src.Substring("data:image/png;base64,".Length);
                                            qrImage = Convert.FromBase64String(base64String);
                                        }
                                        catch { }
                                    }
                                }

                                // Use SIMPLE version that always works
                                var converter = new HtmlToQuestPdfConverter();
                                var pdfBytes = converter.CreateSimpleBusinessCardPdf(name, company, country, qrImage);

                                if (pdfBytes != null && pdfBytes.Length > 0)
                                {
                                    var safeName = request.ParticipantName?.Replace(" ", "_") ?? participantId.ToString();
                                    var fileName = $"{safeName}_ID_Card.pdf";
                                    var entry = zipArchive.CreateEntry(fileName);

                                    using (var entryStream = entry.Open())
                                    {
                                        entryStream.Write(pdfBytes, 0, pdfBytes.Length);
                                    }

                                    generatedCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                // Continue with other participants
                                Console.WriteLine($"Error for {request.QrCode}: {ex.Message}");
                            }
                        }

                        if (generatedCount == 0)
                        {
                            return Json(new { success = false, message = "No PDFs were generated." });
                        }
                    }

                    memoryStream.Position = 0;
                    var zipfileName = $"ID_Cards_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                    return File(memoryStream.ToArray(), "application/zip", zipfileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDFs: {ex.Message}" });
            }
        }

        private async Task<byte[]> ConvertHtmlToPdf(string htmlContent)
        {
            // Using DinkToPdf or similar library
            var converter = new BasicConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            ColorMode = DinkToPdf.ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = DinkToPdf.PaperKind.A6,
            Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 0, Right = 0 }
        },
                Objects = {
            new ObjectSettings()
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontSize = 9, Right = "© " + DateTime.Now.Year }
            }
        }
            };

            return converter.Convert(doc);
        }



    }
}