using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebUI.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index(int eventId)
        {
            return View();
        }

    }
}
