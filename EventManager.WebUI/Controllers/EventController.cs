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

        // Edit existing event (secure, no EventId in URL)
        [HttpPost]
        public async Task<IActionResult> Edit(int eventId)
        {
            if (eventId <= 0) return BadRequest();

            // Store eventId in claims
            await _eventClaimService.SetEventIdClaimAsync(eventId);

            var eventWithTickets = await _eventService.GetEventWithTicketsByIdAsync(eventId);
            if (eventWithTickets == null) return NotFound();

            // Reuse Create view for editing
            return View("Create", eventWithTickets);
        }

        // Save (Create or Update) event
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] EventDto dto)
        {
            if (dto == null)
                return Json(new { success = false, message = "No data received." });

            try
            {
                // Use EventId from claim if available
                int claimEventId = _eventClaimService.GetEventIdFromClaim();
                dto.EventId = claimEventId > 0 ? claimEventId : dto.EventId;

                var eventId = await _eventService.SaveEventAsync(dto);
                return Json(new { success = true, message = "Event saved successfully.", eventId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Delete event
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid event ID." });

            try
            {
                await _eventService.DeleteEventAsync(id);
                return Json(new { success = true, message = "Event deleted successfully." });
            }
            catch (Exception ex)
            {
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
