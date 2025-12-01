using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
