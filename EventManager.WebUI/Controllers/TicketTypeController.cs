using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class TicketTypeController : Controller
    {
        private readonly ITicketTypeService _service;

        public TicketTypeController(ITicketTypeService service)
        {
            _service = service;
        }

        // =====================================================================
        // GET: /TicketType/ByEvent/5
        // =====================================================================
        public async Task<IActionResult> ByEvent(int eventId)
        {
            var list = await _service.GetTicketTypesByEventAsync(eventId);
            return View(list);
        }

        // =====================================================================
        // GET: /TicketType/Create
        // =====================================================================
        public IActionResult Create(int eventId)
        {
            var model = new TicketTypeDto { EventId = eventId };
            return View(model);
        }

        // =====================================================================
        // GET: /TicketType/Edit/5
        // =====================================================================
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.GetTicketTypeByIdAsync(id);
            if (item == null)
                return NotFound();

            return View("Create", item); // reuse the same view for editing
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] TicketTypeDto model)
        {
            if (model == null)
                return Json(new { success = false, message = "No data received." });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            try
            {
                model.UserId = 1;

                await _service.SaveTicketTypeAsync(model);

                return Json(new { success = true, message = "Ticket type saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Invalid ID." });

            try
            {
                await _service.DeleteTicketTypeAsync(id);
                return Json(new { success = true, message = "Ticket deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
