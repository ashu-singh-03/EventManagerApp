using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class TicketParticipantsController : Controller
    {
        private readonly ITicketParticipantsService _service;
        private readonly IEventClaimService _eventClaimService;

        public TicketParticipantsController(
            ITicketParticipantsService service,
            IEventClaimService eventClaimService)
        {
            _service = service;
            _eventClaimService = eventClaimService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("Invalid event");
            return View(id);
        }

        [HttpGet]
        public async Task<IActionResult> LoadParticipants([FromQuery] int ticketTypeId)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return Json(new List<TicketAccessPointAssignmentDto>());

            var accessPoints = await _service.GetParticipantsForTicketTypeAsync(ticketTypeId, eventId);
            return Json(accessPoints);
        }

        // POST: /TicketParticipants/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromForm] SaveParticipantsDto dto)
        {
            if (dto == null || dto.TicketTypeId <= 0)
                return BadRequest(new { message = "Invalid request data" });

            try
            {
                int userId = GetCurrentUserId();
                await _service.SaveParticipantAssignmentsAsync(
                    dto.TicketTypeId,
                    dto.ParticipantIds ?? new List<int>(),
                    userId);

                return Ok(new
                {
                    success = true,
                    message = "Participant assignments saved successfully!",
                    assignedCount = dto.ParticipantIds?.Count ?? 0
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new
                {
                    message = "An error occurred while saving participant assignments",
                    error = ex.Message
                });
            }
        }

        private int GetCurrentUserId()
        {
            return int.TryParse(User.FindFirst("UserId")?.Value, out int userId) ? userId : 1;
        }
    }
  
}