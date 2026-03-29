using Microsoft.AspNetCore.Mvc;
using RedConnect.DAL;
using RedConnect.Interfaces;

namespace RedConnect.Controllers
{
    public class MapController : Controller
    {
        private readonly DonorMapService _mapService;
        private readonly IMongoRepository _mongoRepo;

        public MapController(DonorMapService mapService, IMongoRepository repo)
        {
            _mapService = mapService;
            _mongoRepo = repo;
        }

        public async Task<IActionResult> Donors(string bloodGroup = null)
        {
            var donors = await _mapService.GetActiveDonorsAsync();
            return View(donors);
        }
    }
}
