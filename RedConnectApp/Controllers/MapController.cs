using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using RedConnect.DAL;
using RedConnect.Models;
using RedConnect.ViewModels;

namespace RedConnect.Controllers
{
    
    public class MapController : Controller
    {
        private readonly DonorMapService _mapService;
        public MapController(DonorMapService mapService)
        {
            _mapService = mapService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Donors()
        {
            var donors = await _mapService.GetActiveDonorsAsync();
            return View(donors);
        }
    }

}

