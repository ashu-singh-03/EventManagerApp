using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using Microsoft.AspNetCore.Mvc;
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
    }
}