using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using RedConnect.DAL;
using RedConnect.Models;
using RedConnect.ViewModels;

namespace RedConnectApp.Controllers
{
    
    public class DonorController : Controller
    {
        private readonly DonorMapService _mapService;
        public DonorController(DonorMapService mapService)
        {
            _mapService = mapService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DonorMap()
        {
            var donors = await _mapService.GetActiveDonorsAsync();
            return View(donors);
        }
    }

}

