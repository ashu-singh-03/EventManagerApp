using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class TicketAccessPointController : Controller
    {
        private readonly ITicketAccessPointService _service;
        private readonly IEventClaimService _eventClaimService;

        public TicketAccessPointController(
            ITicketAccessPointService service,
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
        public async Task<IActionResult> Manage(int id)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("Invalid event");
            return View(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetTicketTypes([FromQuery] int eventId = 0)
        {
            if (eventId == 0)
                eventId = _eventClaimService.GetEventIdFromClaim();

            if (eventId == 0)
                return Json(new List<TicketTypeDto>());

            var ticketTypes = await _service.GetTicketTypesAsync(eventId);
            return Json(ticketTypes);
        }

        [HttpGet]
        public async Task<IActionResult> LoadAccessPoints([FromQuery] int ticketTypeId)
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0)
                return Json(new List<TicketAccessPointAssignmentDto>());

            var accessPoints = await _service.GetAccessPointsForTicketTypeAsync(ticketTypeId, eventId);
            return Json(accessPoints);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromForm] SaveAssignmentsDto dto)
        {
            if (dto == null || dto.TicketTypeId <= 0)
                return BadRequest(new { message = "Invalid request data" });

            try
            {
                int userId = GetCurrentUserId();
                await _service.SaveAssignmentsAsync(dto.TicketTypeId, dto.AccessPointIds, userId);

                return Ok(new
                {
                    success = true,
                    message = "Assignments saved successfully!"
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
                    message = "An error occurred while saving assignments"
                });
            }
        }
        private int GetCurrentUserId()
        {
            return int.TryParse(User.FindFirst("UserId")?.Value, out int userId) ? userId : 1;
        }
    }

}