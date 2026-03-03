using Microsoft.AspNetCore.Mvc;
using RedConnect.DAL;
using RedConnect.Models;
using RedConnect.ViewModels;
using RedConnectApp.DAL;

namespace RedConnect.Controllers
{
    public class PortalController : Controller
    {
        private readonly MongoRepository _repo;
        private readonly MSSQLDBContext _context;

        public PortalController(MongoRepository repo, MSSQLDBContext context)
        {
            _repo = repo;
            _context = context;
        }

        // --- Donor List (all donors) ---
        public async Task<IActionResult> DonorList()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var donors = await _repo.GetAllDonorsAsync();

            var donorList = donors.Select(x => new DonorListViewModel
            {
                UserId            = x.UserId,
                Name              = x.UserDetails?.Name,
                BloodGroup        = x.BloodGroup,
                LocationText      = x.LocationText,
                Verified          = x.Verified,
                DocumentsUploaded = x.DocumentsUploaded,
                ReportCount       = x.MedicalReports?.Count    ?? 0,
                RejectedCount     = x.MedicalReports?.Count(r => r.Status == "Rejected")  ?? 0,
                ApprovedCount     = x.MedicalReports?.Count(r => r.Status == "Approved")  ?? 0
            }).ToList();

            return View(donorList);
        }

        // --- Verify donor ---
        [HttpPost]
        public async Task<IActionResult> Verify(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            await _repo.VerifyDonorAsync(userId);
            return RedirectToAction("DonorList");
        }

        // --- View donor medical documents ---
        public async Task<IActionResult> ViewDocuments(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var mongoUser = await _repo.GetMongoUserAsync(userId);
            if (mongoUser == null) return NotFound();

            ViewBag.DonorName = mongoUser.UserDetails?.Name ?? $"Donor #{userId}";
            ViewBag.UserId    = userId;
            ViewBag.Verified  = mongoUser.Verified;

            return View(mongoUser.MedicalReports ?? new List<MedicalReport>());
        }

        // --- Approve a single document ---
        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int userId, int docIndex)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await _repo.UpdateMedicalReportStatusAsync(userId, docIndex, "Approved");
            TempData["Success"] = "Document approved.";
            return RedirectToAction("ViewDocuments", new { userId });
        }

        // --- Reject a single document ---
        [HttpPost]
        public async Task<IActionResult> RejectDocument(int userId, int docIndex, string reason)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await _repo.UpdateMedicalReportStatusAsync(userId, docIndex, "Rejected",
                string.IsNullOrWhiteSpace(reason) ? "Does not meet requirements." : reason);
            TempData["Success"] = "Document rejected.";
            return RedirectToAction("ViewDocuments", new { userId });
        }

        // --- Deactivate donor ---
        [HttpPost]
        public async Task<IActionResult> Deactivate(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            await _repo.DeactivateUserAsync(userId);
            return RedirectToAction("DonorList");
        }

        // --- Blood Bank list ---
        public async Task<IActionResult> BankList()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var banks = await _repo.GetAllBloodBanksAsync();

            var vm = banks.Select(b => new BloodBankListViewModel
            {
                LocationName = b.LocationName,
                Address      = b.Address,
                StaffCount   = b.UserIds?.Count ?? 0,
                CreatedOn    = b.CreatedOn
            }).ToList();

            return View(vm);
        }

        // --- Register Blood Bank (GET) ---
        public IActionResult RegisterBank()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var vm = new BloodBankViewModel
            {
                UserTypes = _context.UserType.ToList()
            };
            return View(vm);
        }

        // --- Register Blood Bank (POST) ---
        [HttpPost]
        public async Task<IActionResult> RegisterBank(BloodBankViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (await _repo.EmailExistsAsync(model.StaffEmail))
            {
                ModelState.AddModelError("StaffEmail", "Email already exists.");
                model.UserTypes = _context.UserType.ToList();
                return View(model);
            }

            await _repo.CreateOrUpdateBloodBankAsync(
                model.LocationName,
                model.Address,
                model.StaffEmail,
                model.Password,
                model.SelectedUserTypeId,
                model.Lat,
                model.Lng,
                model.LocationSearch);

            TempData["Success"] = "Blood bank registered successfully.";
            return RedirectToAction("BankList");
        }

        private bool IsAdmin()
        {
            var userTypeId = HttpContext.Session.GetInt32("UserTypeId");
            return userTypeId != null && userTypeId != 0;
        }
    }
}
