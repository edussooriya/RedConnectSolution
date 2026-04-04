using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RedConnect.DAL;
using RedConnect.Interfaces;
using RedConnect.Models;
using RedConnect.ViewModels;
using RedConnectApp.Enums;
using RedConnectApp.Services;

namespace RedConnect.Controllers;

public class AccountController : BaseController
{
    private readonly PasswordResetService _passwordResetService;
    private readonly IUserService _userService;
    private readonly IMedicalReportService _medicalReportService;
    private readonly IBloodBankService _bankService;

    public AccountController(PasswordResetService passwordResetService,
        IUserService userService, IMedicalReportService medicalReportService, IBloodBankService bankService)
    {
        _passwordResetService = passwordResetService;
        _userService = userService;
        _medicalReportService = medicalReportService;
        _bankService = bankService;
    }

    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string email, string password,
        string name, string address, string nic,
        double donatedLng, double donatedLat,
        double availableLng, double availableLat, string locationText, string phone,
        GenderEnum gender, string bloodGroup, int userTypeId = 0)
    {
        //REQUIRED VALIDATION
        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError("email", "Email is required");

        if (!new EmailAddressAttribute().IsValid(email))
            ModelState.AddModelError("email", "Invalid email format");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            ModelState.AddModelError("password", "Password must be at least 8 characters");

        if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$"))
            ModelState.AddModelError("password", "Password must contain uppercase, lowercase, number, and special character");

        if (string.IsNullOrWhiteSpace(name))
            ModelState.AddModelError("name", "Name is required");

        if (string.IsNullOrWhiteSpace(nic))
            ModelState.AddModelError("nic", "NIC is required");


        //
         if (await _userService.EmailExistsAsync(email))
            ModelState.AddModelError("email", "Email already exists");

        // 
        if (!ModelState.IsValid)
        {
            return View(); // IMPORTANT: return same view
        }



        await _userService.RegisterAsync(userTypeId, email, password,
            name, address, nic,
            donatedLng, donatedLat,
            availableLng, availableLat, locationText, phone, gender, bloodGroup);

        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Login()
    {
        var totalDonors = await _userService.GetAllUsersAsync(true, 0); //Active and userType 0 - Donors
        var totalBanks = await _bankService.GetBloodBankCount();
        ViewBag.ActiveDonors = totalDonors.Count;
        ViewBag.TotalBanks   = totalBanks;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userService.LoginAsync(email, password);

        if (user == null)
        {
            ViewBag.Error = "Invalid email or password";
            return View();
        }

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetInt32("UserTypeId", user.UserTypeId);

        // Donors must upload documents and be verified before accessing the app
        if (user.UserTypeId == 0)
        {
            var mongoUser = await _userService.GetUserById(user.UserId);
            if (mongoUser == null || !mongoUser.DocumentsUploaded)
                return RedirectToAction("UploadDocuments");
            if (!mongoUser.Verified)
                return RedirectToAction("PendingVerification");
        }

        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var sessionUserId = HttpContext.Session.GetInt32("UserId");
        if (sessionUserId == null) return RedirectToAction("Login");

        var sqlUser = await _userService.GetUserById(sessionUserId.Value,true);
        if (sqlUser == null) return NotFound();

        var mongoUser = await _userService.GetUserById(sessionUserId.Value);

        var model = new UserViewModel
        {
            UserId     = sqlUser.UserId,
            UserTypeId = sqlUser.UserTypeId,
            Email      = sqlUser.Email,
            Active     = sqlUser.Active,

            Name    = mongoUser?.UserDetails?.Name,
            Address = mongoUser?.UserDetails?.Address,
            NIC     = mongoUser?.UserDetails?.NIC,
            Phone   = mongoUser?.UserDetails?.Phone,

            DonatedLng = mongoUser?.DonatedLocation?.Coordinates?[0] ?? 0,
            DonatedLat = mongoUser?.DonatedLocation?.Coordinates?[1] ?? 0,

            AvailableLng = mongoUser?.AvailableLocation?.Coordinates?[0] ?? 0,
            AvailableLat = mongoUser?.AvailableLocation?.Coordinates?[1] ?? 0,

            LocationText = mongoUser?.LocationText,
            Concent      = mongoUser?.Concent ?? false,
            BloodGroup   = mongoUser?.BloodGroup ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserViewModel model)
    {
        await _userService.UpdateAsync(
            model.UserId, model.UserTypeId, model.Email, model.Active,
            model.Name, model.Address, model.NIC, model.Phone,
            model.DonatedLng, model.DonatedLat,
            model.AvailableLng, model.AvailableLat,
            model.LocationText, model.Concent, model.BloodGroup);

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction("Edit", new { id = model.UserId });
    }

    [HttpGet]
    public async Task<IActionResult> UploadDocuments()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        // If already uploaded → go to pending
        var mongoUser = await _userService.GetUserById(userId.Value);
        if (mongoUser?.DocumentsUploaded == true)
            return RedirectToAction("PendingVerification");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadDocuments(
        IFormFile report1, IFormFile report2, IFormFile report3)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        var files = new[] { report1, report2, report3 };
        if (files.Any(f => f == null || f.Length == 0))
        {
            ViewBag.Error = "Please upload all 3 medical report files.";
            return View();
        }

        var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        if (files.Any(f => !allowed.Contains(Path.GetExtension(f.FileName).ToLower())))
        {
            ViewBag.Error = "Only PDF, JPG, and PNG files are accepted.";
            return View();
        }

        var dir = Path.Combine(Directory.GetCurrentDirectory(),
                               "wwwroot", "uploads", "medical", userId.ToString());
        Directory.CreateDirectory(dir);

        var saved = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            var ext      = Path.GetExtension(files[i].FileName).ToLower();
            var fileName = $"report{i + 1}{ext}";
            var fullPath = Path.Combine(dir, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await files[i].CopyToAsync(stream);
            saved.Add($"/uploads/medical/{userId}/{fileName}");
        }

        await _medicalReportService.SaveMedicalReportsAsync(userId.Value, saved);
        return RedirectToAction("PendingVerification");
    }

    [HttpGet]
    public async Task<IActionResult> PendingVerification()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        // If admin just verified the user, let them through
        var mongoUser = await _userService.GetUserById(userId.Value);
        if (mongoUser?.Verified == true)
            return RedirectToAction("Index", "Dashboard");

        return View(mongoUser?.MedicalReports ?? new List<MedicalReport>());
    }

    [HttpPost]
    public async Task<IActionResult> ReuploadDocument(int docIndex, IFormFile file)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file to upload.";
            return RedirectToAction("PendingVerification");
        }

        var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var ext = System.IO.Path.GetExtension(file.FileName).ToLower();
        if (!allowed.Contains(ext))
        {
            TempData["Error"] = "Only PDF, JPG, and PNG files are accepted.";
            return RedirectToAction("PendingVerification");
        }

        var dir = System.IO.Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads", "medical", userId.ToString());
        System.IO.Directory.CreateDirectory(dir);

        var fileName = $"report{docIndex + 1}{ext}";
        var fullPath = System.IO.Path.Combine(dir, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

       await _medicalReportService.ReuploadMedicalReportAsync(
           userId.Value, docIndex, $"/uploads/medical/{userId}/{fileName}");

        TempData["Success"] = "Document re-uploaded. Admin will review it shortly.";
        return RedirectToAction("PendingVerification");
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        if (model.NewPassword != model.ConfirmPassword)
        {
            ViewBag.Error = "New passwords do not match.";
            return View(model);
        }

        var isValid = await _userService.VerifyPasswordAsync(userId.Value, model.CurrentPassword);
        if (!isValid)
        {
            ViewBag.Error = "Current password is incorrect.";
            return View(model);
        }

        await _userService.ChangePasswordAsync(userId.Value, model.NewPassword);
        ViewBag.Success = "Password changed successfully.";
        return View();
    }
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(string email)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        _passwordResetService.SendResetLink(email, baseUrl);

        ViewBag.Message = "Reset link sent to email";

        return View();
    }

    public IActionResult ResetPassword(string token)
    {
        ViewBag.Token = token;
        return View();
    }

    [HttpPost]
    public IActionResult ResetPassword(string token, string password)
    {
        var result = _passwordResetService.ResetPassword(token, password);

        if (!result)
        {
            ViewBag.Error = "Invalid or expired token";
            return View();
        }

        return RedirectToAction("Login");
    }
}
