using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static EventManager.Application.DTOs.ScanDtos;
using Microsoft.AspNetCore.Hosting;

namespace EventManager.WebUI.Controllers
{
    public class QrConfigurationController : Controller
    {
        private readonly IScanService _scanService;
        private readonly IEventClaimService _eventClaimService;

        private readonly IWebHostEnvironment _hostingEnvironment;
        public QrConfigurationController(
            IScanService scanService,
            IEventClaimService eventClaimService,
            IWebHostEnvironment hostingEnvironment)
        {
            _scanService = scanService;
            _eventClaimService = eventClaimService;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public IActionResult Index()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("Invalid event");

            ViewBag.EventId = eventId;  // This is already set
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AdminScanLog(int? accessPointId = null)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return BadRequest("Invalid event");

            ViewBag.EventId = eventId;
            ViewBag.AccessPointId = accessPointId;
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetScanLog(int accessPointId)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return Json(new { success = false, message = "Invalid event" });

        
            var result = await _scanService.ScanLogDetailsAsync(eventId, accessPointId);

            // CHANGE: Return JSON instead of PartialView
            return Json(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetStats(int accessPointId)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return Json(new { success = false, message = "Invalid event" });

            var stats = await _scanService.GetScanStatisticsAsync(eventId, accessPointId);

            return Json(new
            {
                success = true,
                totalScans = stats.TotalScans,
                validScans = stats.ValidScans,
                invalidScans = stats.InvalidScans,
                duplicateScans = stats.DuplicateScans
            });
        }
        [HttpPost]
        public async Task<JsonResult> ProcessScan([FromBody] ScanRequestDto request)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return Json(new { success = false, message = "Invalid event" });

            // Check if this is a reprint request
            bool isReprint = HttpContext.Request.Query["isReprint"] == "true";

            var fontFolder = Path.Combine(_hostingEnvironment.WebRootPath, "arial");

            var result = await _scanService.ProcessScanAsync(
                eventId,
                request,
                request.IsPrintCenter,
                isReprint, // PASS isReprint parameter
                fontFolder
            );

            string idCardBase64 = null;
            if (result.pdfBytes != null && result.pdfBytes.Length > 0)
            {
                idCardBase64 = Convert.ToBase64String(result.pdfBytes);
            }

            return Json(new
            {
                success = result.Success,
                ticketId = result.TicketId,
                holderName = result.HolderName,
                status = result.Status,
                scanTime = result.ScanTime,
                accessPoint = result.AccessPoint,
                message = result.Message,
                participantId = result.ParticipantId,
                isPrintCenter = result.IsPrintCenter,
                idCardBase64 = idCardBase64,
                validationStatus = result.Status,
                validationMessage = result.Message,
                fullName = result.FullName,
                participantCode = result.ParticipantCode,
                company = result.Company,
                country = result.Country,
                isReprint = isReprint  // Return for debugging
            });
        }


    }
}