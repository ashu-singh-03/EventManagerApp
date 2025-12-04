using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebUI.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IParticipantService _service;

        public ParticipantController(IParticipantService service)
        {
            _service = service;
        }

        // GET: /Participant?eventId=123
        public async Task<IActionResult> Index(int eventId)
        {
            // Pass EventId to ViewBag for the hidden field in the form
            ViewBag.EventId = eventId;

            var participants = await _service.GetParticipantsByEventAsync(eventId);
            return View(participants);
        }

        // GET: /Participant/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var participant = await _service.GetParticipantByIdAsync(id);
            if (participant == null) return NotFound();
            return View(participant);
        }

        // POST: /Participant/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ParticipantDto dto)
        {
            if (dto == null)
                return BadRequest(new { error = "dto is required" });

            // Extra check to ensure EventId is valid
            if (dto.EventId <= 0)
                return BadRequest(new { error = "EventId must be provided and greater than 0" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.SaveParticipantAsync(dto);
            return Ok(new { message = "Participant saved successfully" });
        }

        // POST: /Participant/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id, int eventId)
        {
            await _service.DeleteParticipantAsync(id);
            // Redirect back to the Index of the same event
            return RedirectToAction(nameof(Index), new { eventId });
        }
    }
}
