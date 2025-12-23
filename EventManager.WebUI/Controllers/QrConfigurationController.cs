using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static EventManager.Application.DTOs.ScanDtos;

namespace EventManager.WebUI.Controllers
{
    public class QrConfigurationController : Controller
    {
        private readonly IScanService _scanService;
        private readonly IEventClaimService _eventClaimService;

        public QrConfigurationController(
            IScanService scanService,
            IEventClaimService eventClaimService)
        {
            _scanService = scanService;
            _eventClaimService = eventClaimService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("Invalid event");

            ViewBag.EventId = eventId;
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

            var result = await _scanService.ProcessScanAsync(eventId, request, request.IsPrintCenter);

            return Json(new
            {
                success = result.Success, // Already set correctly by service
                ticketId = result.TicketId,
                holderName = result.HolderName,
                status = result.Status, // Validation status from stored procedure
                scanTime = result.ScanTime,
                accessPoint = result.AccessPoint,
                message = result.Message, // Validation message from stored procedure
                participantId = result.ParticipantId,
                isPrintCenter = result.IsPrintCenter,
                idCardHtml = result.IdCardHtml,
                // These are redundant but you can keep them
                validationStatus = result.Status,
                validationMessage = result.Message,
                fullName = result.HolderName,
                participantCode = result.TicketId
            });
        }

        
    }
}