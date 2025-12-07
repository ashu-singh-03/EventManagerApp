using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class AccessPointController : Controller
    {
        private readonly IAccessPointService _service;
        private readonly IEventClaimService _eventClaimService;

        public AccessPointController(IAccessPointService service, IEventClaimService eventClaimService)
        {
            _service = service;
            _eventClaimService = eventClaimService;
        }

        // LOAD PAGE (HTML)
        public async Task<IActionResult> Index()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest();

            ViewBag.EventId = eventId;

            var accessPoints = await _service.GetAccessPointsByEventAsync(eventId);
            return View(accessPoints);
        }

        // GET ONE ACCESS POINT (JSON for AJAX)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var accessPoint = await _service.GetAccessPointByIdAsync(id);
            if (accessPoint == null)
                return NotFound();
            return Json(accessPoint);
        }

        // GET ALL BY EVENT (JSON for AJAX)
        [HttpGet]
        public async Task<IActionResult> LoadAccessPoints()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest();

            var accessPoints = await _service.GetAccessPointsByEventAsync(eventId);
            return Json(accessPoints);
        }

        // SAVE (INSERT/UPDATE) - JSON
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] AccessPointDto dto)
        {
            if (dto == null)
                return BadRequest(new { error = "DTO is required" });

            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest(new { error = "Invalid Event" });

            dto.EventId = eventId; // set EventId from claim

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Name is required" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.SaveAccessPointAsync(dto);
            return Ok(new { message = "Access point saved successfully" });
        }

        // DELETE (SOFT DELETE) - JSON
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAccessPointAsync(id);
            return Ok(new { message = "Access point deleted successfully" });
        }
    }
}
