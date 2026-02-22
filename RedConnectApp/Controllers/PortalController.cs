using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using RedConnect.DAL;
using RedConnect.Models;
using RedConnect.ViewModels;


namespace RedConnect.Controllers
{
    public class PortalController : Controller
    {
        private readonly UserRepository _repo;
        public PortalController(UserRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> DonorList()
        {
            var donors = await _repo.GetUnverifiedDonorsAsync();

            var donorList = donors.Select(x => new DonorListViewModel
            {
                UserId = x.UserId,
                Name = x.UserDetails.Name,
                BloodGroup = x.BloodGroup,
                LocationText = x.LocationText,
                Verified = x.Verified
            }).ToList();
            return View(donorList);
        }


        [HttpPost]
        public async Task<IActionResult> Verify(int userId)
        {
            await _repo.VerifyDonorAsync(userId);
            return RedirectToAction("DonorList");
        }
    }
}
