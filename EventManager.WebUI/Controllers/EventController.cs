using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebUI.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Event> events;

            try
            {
                events = await _eventService.GetAllEventsAsync();
            }
            catch (Exception ex)
            {
                events = new List<Event>();
            }

            return View(events);
        }


        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Event newEvent)
        {
            if (newEvent == null)
                return Json(new { success = false, message = "No data received." });

            if (ModelState.IsValid)
            {
                newEvent.CreatedBy = "1"; // Replace with current user
                newEvent.CreatedAt = DateTime.Now; // Example
                await _eventService.AddEventAsync(newEvent);

                return Json(new { success = true, message = "Event saved successfully." });
            }

            return Json(new { success = false, message = "Invalid event data." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            try
            {
                await _eventService.DeleteEventAsync(id); // Mark IsActive = 0 in DB
                return Json(new { success = true, message = "Event deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
