using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ITicketTypeService _ticketService;

        public EventController(IEventService eventService, ITicketTypeService ticketService)
        {
            _eventService = eventService;
            _ticketService = ticketService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest();

            var eventWithTickets = await _eventService.GetEventWithTicketsByIdAsync(id);
            if (eventWithTickets == null) return NotFound();

            return View("Create", eventWithTickets);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] EventDto dto)
        {
            if (dto == null)
                return Json(new { success = false, message = "No data received." });

            try
            {
                var eventId = await _eventService.SaveEventAsync(dto);
                return Json(new { success = true, message = "Event saved successfully.", eventId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


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
    }
}
