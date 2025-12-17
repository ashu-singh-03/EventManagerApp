using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
    }
}