using Microsoft.AspNetCore.Mvc;
using RedConnect.DAL;
using RedConnect.Interfaces;
using RedConnect.Services;
using RedConnect.ViewModels;

namespace RedConnect.Controllers;

public class DashboardController : Controller
{
    private readonly IUserService _userService;
    private readonly DashboardService _dashboardService;

    public DashboardController(IUserService userService, DashboardService dashboardService)
    {
        _userService = userService;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Index", "Landing");

        var mongoUser = await _userService.GetUserById(userId.Value);

        // Donors must complete document upload + verification before accessing dashboard
        var userTypeId = HttpContext.Session.GetInt32("UserTypeId") ?? 0;
        if (userTypeId == 0)
        {
            if (mongoUser == null || !mongoUser.DocumentsUploaded)
                return RedirectToAction("UploadDocuments", "Account");
            if (!mongoUser.Verified)
                return RedirectToAction("PendingVerification", "Account");
        }

        var (totalDonors, verifiedDonors, totalBanks) = await _dashboardService.GetDashboardStatsAsync();

        var vm = new DashboardViewModel
        {
            TotalDonors    = totalDonors,
            VerifiedDonors = verifiedDonors,
            PendingDonors  = totalDonors - verifiedDonors,
            TotalBanks     = totalBanks,
            UserName       = mongoUser?.UserDetails?.Name ?? "User",
            BloodGroup     = mongoUser?.BloodGroup,
            UserTypeId     = HttpContext.Session.GetInt32("UserTypeId") ?? 0
        };

        return View(vm);
    }
}
