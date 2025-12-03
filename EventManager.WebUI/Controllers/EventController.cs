using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // GET: /Event
        public async Task<IActionResult> Index()
        {
            List<Event> events = new();
            try
            {
                var allEvents = await _eventService.GetAllEventsAsync();
                if (allEvents != null)
                    events = new List<Event>(allEvents);
            }
            catch (Exception ex)
            {
                // Optionally log the exception
            }
            return View(events);
        }

        // GET: /Event/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(); // Empty model for new event
        }

        // GET: /Event/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var evt = await _eventService.GetEventByIdAsync(id);
            if (evt == null)
                return NotFound();

            return View("Create", evt); // Reuse Create view for editing
        }

        // POST: /Event/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Event evt)
        {
            if (evt == null)
                return Json(new { success = false, message = "No data received." });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            try
            {
                await _eventService.SaveEventAsync(evt); // Calls SP for Insert/Update
                return Json(new { success = true, message = "Event saved successfully." });
            }
            catch (Exception ex)
            {
                // Optionally log exception
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Event/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Invalid event ID." });

            try
            {
                await _eventService.DeleteEventAsync(id); // Soft delete
                return Json(new { success = true, message = "Event deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
