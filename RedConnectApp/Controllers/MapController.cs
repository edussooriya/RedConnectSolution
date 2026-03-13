using Microsoft.AspNetCore.Mvc;
using RedConnect.DAL;

namespace RedConnect.Controllers
{
    public class MapController : Controller
    {
        private readonly DonorMapService _mapService;
        private readonly MongoRepository _repo;

        public MapController(DonorMapService mapService, MongoRepository repo)
        {
            _mapService = mapService;
            _repo       = repo;
        }

        public async Task<IActionResult> Donors(string bloodGroup = null)
        {
            //var userId     = HttpContext.Session.GetInt32("UserId");
            //var userTypeId = HttpContext.Session.GetInt32("UserTypeId") ?? 0;

            //if (userId == null) return RedirectToAction("Login", "Account");

            //// Donors must be verified to view the map
            //if (userTypeId == 0)
            //{
            //    var mongoUser = await _repo.GetMongoUserAsync(userId.Value);
            //    if (mongoUser == null || !mongoUser.DocumentsUploaded)
            //        return RedirectToAction("UploadDocuments", "Account");
            //    if (!mongoUser.Verified)
            //        return RedirectToAction("PendingVerification", "Account");
            //}

            //var donors = await _mapService.GetActiveDonorsAsync(bloodGroup);

            //// Load blood banks that have a mapped location
            //var banks = await _repo.GetAllBloodBanksAsync();
            //ViewBag.BloodBanks = banks
            //    .Where(b => b.Lat != 0 && b.Lng != 0)
            //    .Select(b => new { b.LocationName, b.Address, b.Lat, b.Lng })
            //    .ToList();

            //ViewBag.BloodGroup = bloodGroup;
            //return View(donors);

            var donors = await _mapService.GetActiveDonorsAsync();
            return View(donors);
        }
    }
}
