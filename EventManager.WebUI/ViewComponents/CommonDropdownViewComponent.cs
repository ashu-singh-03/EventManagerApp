using EventManager.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.WebUI.ViewComponents
{
    public class CommonDropdownViewComponent : ViewComponent
    {
        private readonly CommonRepository _repo;
        private readonly ILogger<CommonDropdownViewComponent> _logger;

        public CommonDropdownViewComponent(
            CommonRepository repo,
            ILogger<CommonDropdownViewComponent> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string id,
            string flag,
            string placeholder,
            int? eventId = null)
        {
            _logger.LogInformation($"ViewComponent called: {flag}, eventId={eventId}");

            try
            {
                List<DropdownItemDto> items = await _repo.GetDropdownAsync(flag, eventId);
                _logger.LogInformation($"Got {items?.Count ?? 0} items");

                ViewBag.Id = id;
                ViewBag.Placeholder = placeholder;

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ViewComponent");
                // Return empty result instead of throwing
                ViewBag.Id = id;
                ViewBag.Placeholder = placeholder;
                return View(new List<DropdownItemDto>());
            }
        }
    }
}