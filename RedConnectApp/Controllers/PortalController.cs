using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using RedConnect.DAL;
using RedConnect.Models;
using RedConnect.ViewModels;
using RedConnectApp.DAL;


namespace RedConnect.Controllers
{
    enum UserType 
    { 
        GeneralUser
    }
    public class PortalController : Controller
    {
        private readonly MongoRepository _repo;
        private readonly MSSQLDBContext _context;

        public PortalController(MongoRepository repo, MSSQLDBContext context)
        {
            _repo = repo;
            _context = context;
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

        [HttpPost]
        public async Task<IActionResult> RegisterBank(BloodBankViewModel model)
        {
            if (await _repo.EmailExistsAsync(model.StaffEmail))
            {
                ModelState.AddModelError("StaffEmail", "Email already exists.");
                model.UserTypes = _context.UserType.ToList();
                return View(model);
            }
            else 
            {
                await _repo.CreateOrUpdateBloodBankAsync(
                       model.LocationName,
                       model.Address,
                       model.StaffEmail,
                       model.Password,
                       model.SelectedUserTypeId
                   );

                return RedirectToAction("RegisterBank");
            }

               
        }

        public IActionResult RegisterBank()
        {
         
            var vm = new BloodBankViewModel
            {
                UserTypes = _context.UserType.ToList()
            };

            return View(vm);
        }

        //private IActionResult CheckValidity()
        //{
        //    if (HttpContext.Session.GetInt32("UserTypeId") == (int)UserType.GeneralUser)
        //    {
        //        return RedirectToAction("RegisterBank");
        //    }
           
        //}
    }
}
