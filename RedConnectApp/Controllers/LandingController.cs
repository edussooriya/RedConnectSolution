using Microsoft.AspNetCore.Mvc;

namespace RedConnect.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
