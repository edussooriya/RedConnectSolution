using Microsoft.AspNetCore.Mvc;

namespace RedConnect.Controllers
{
    public class LandingController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
