using DinkToPdf;
using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Domain.Entities;
using EventManager.WebUI.ViewComponents;
using EventManager.WebUI.ViewComponents.EventManager.WebUI.ViewComponents;
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
                if (eventId == 0)
                    return Json(new { success = false, message = "Invalid event" });

                // Convert QR code to participant ID (assuming QR code contains the participant ID)
                if (!int.TryParse(request.QrCode, out int participantId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid QR code format. Could not parse participant ID."
                    });
                }

                var result = await _service.GenerateIdCardAsync(eventId, participantId);

                return Json(new
                {
                    success = result.Success,
                    idCardHtml = result.IdCardHtml,  // Added this
                    message = result.ValidationMessage,
                    participantId = result.ParticipantId,
                    fullName = result.FullName,
                    participantCode = result.ParticipantCode
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error generating ID card: {ex.Message}"
                });
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
                        foreach (var request in requests)
                        {
                            if (!int.TryParse(request.QrCode, out int participantId))
                            {
                                 
                            }
                            // Generate each ID card
                            // var idCardResult = await GenerateSingleIdCard(request.QrCode, request.IsPrintCenter);
                            var idCardResult = await _service.GenerateIdCardAsync(eventId, participantId);
                            if (idCardResult.Success)
                            {
                                // Convert HTML to PDF (you'll need a library like iTextSharp, DinkToPdf, etc.)
                                 HtmlToQuestPdfConverter htmlToQuestPdfConverter = new HtmlToQuestPdfConverter();
                                //BusinessCardPdfGenerator businessCardPdfGenerator = new BusinessCardPdfGenerator(request.ParticipantName,"as","");
                                // var pdfDocVal =  businessCardPdfGenerator.GeneratePdfAsync();

                                  var pdfDocVal = htmlToQuestPdfConverter.ConvertHtmlToDocument(idCardResult.IdCardHtml,true);

                                 
                                var pdfBytes = pdfDocVal.GeneratePdf();
                                // Create ZIP entry
                                var fileName = $"{request.QrCode.Replace(" ", "_")}_ID_Card.pdf";
                                var entry = zipArchive.CreateEntry(fileName);

                                using (var entryStream = entry.Open())
                                {
                                    entryStream.Write(pdfBytes, 0, pdfBytes.Length);
                                }
                            }
                        }
                    }

                    memoryStream.Position = 0;
                    var zipfileName = $"ID_Cards_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                    //return File(memoryStream.ToArray(), "application/zip", $"id_cards_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                    return File(memoryStream.ToArray(),"application/zip", zipfileName);

                }
            }
            catch (Exception ex)
            {
              //  _logger.LogError(ex, "Error generating bulk ID cards");
                return Json(new { success = false, message = "Error generating PDFs" });
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