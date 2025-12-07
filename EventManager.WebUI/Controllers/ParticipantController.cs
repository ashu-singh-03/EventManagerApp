using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IParticipantService _service;
        private readonly IEventClaimService _eventClaimService;

        public ParticipantController(IParticipantService service, IEventClaimService eventClaimService)
        {
            _service = service;
            _eventClaimService = eventClaimService;
        }

        // LOAD PAGE (HTML)
        public async Task<IActionResult> Index()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("EventId claim not set.");

            var participants = await _service.GetParticipantsByEventAsync(eventId);
            return View(participants);
        }

        // RETURN PARTICIPANTS AS JSON (for table AJAX)
        [HttpGet]
        public async Task<IActionResult> LoadParticipants()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("EventId claim not set.");

            var participants = await _service.GetParticipantsByEventAsync(eventId);
            return Json(participants);
        }

        // GET ONE PARTICIPANT
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var participant = await _service.GetParticipantByIdAsync(id);
            if (participant == null)
                return NotFound();

            return Json(participant);
        }

        // SAVE (INSERT/UPDATE)
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ParticipantDto dto)
        {
            dto.EventId = _eventClaimService.GetEventIdFromClaim();
            if (dto.EventId <= 0)
                return BadRequest(new { error = "EventId must be greater than 0" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.SaveParticipantAsync(dto);

            return Ok(new { message = "Participant saved successfully" });
        }


        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteParticipantAsync(id);

            return Ok(new { message = "Deleted successfully" });
        }
    }
}
