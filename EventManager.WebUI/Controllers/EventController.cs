using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ITicketTypeService _ticketService;
        private readonly IEventClaimService _eventClaimService;
        public EventController(IEventService eventService, ITicketTypeService ticketService, IEventClaimService eventClaimService)
        {
            _eventService = eventService;
            _ticketService = ticketService;
            _eventClaimService = eventClaimService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }

        // Create new event
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) 
        {
            if (id <= 0) return BadRequest();

            // Store eventId in claims (your security logic)
            await _eventClaimService.SetEventIdClaimAsync(id);

            var eventWithTickets = await _eventService.GetEventWithTicketsByIdAsync(id);
            if (eventWithTickets == null) return NotFound();

            // Reuse Create view for editing
            return View("Create", eventWithTickets);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] EventDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "No data received." });

            try
            {
                if (dto.EventId > 0)
                {
                    // Existing event: use claim to validate/update
                    int claimEventId = _eventClaimService.GetEventIdFromClaim();
                    if (claimEventId > 0)
                    {
                        dto.EventId = claimEventId; // Ensure update is allowed for this user
                    }
                }
                // else: new event, ignore claim and let SaveEventAsync insert a new record

                var eventId = await _eventService.SaveEventAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = dto.EventId > 0 ? "Event updated successfully." : "Event created successfully.",
                    eventId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while saving the event.",
                    detail = ex.Message
                });
            }
        }


        // Delete event
        [HttpPost]
        public async Task<IActionResult> Delete(int id) // [FromBody] removed for compatibility
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid event ID." });

            try
            {
                await _eventService.DeleteEventAsync(id);
                return Json(new { success = true, message = "Event deleted successfully." });
            }
            catch (Exception ex)
            {
                // Logs the error and returns it to the UI toast
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SetEventAndManage(int eventId)
        {
            if (eventId <= 0) return BadRequest();

            // Store eventId in claims
            await _eventClaimService.SetEventIdClaimAsync(eventId);

            // Redirect to Participant/Index which will read claim
            return RedirectToAction("Index", "Participant");
        }
    }
}
