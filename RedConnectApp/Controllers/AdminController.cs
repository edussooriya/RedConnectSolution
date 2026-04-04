using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RedConnect.DAL;
using RedConnect.Interfaces;
using RedConnect.Models;
using RedConnect.ViewModels;
using RedConnectApp.DAL;
using RedConnectApp.Models;
using RedConnectApp.ViewModels;

namespace RedConnect.Controllers;

public class AdminController : BaseController
{
    
    private readonly DonorMapService _mapService;
    private readonly IUserService _userService;


    public AdminController(DonorMapService mapService,IUserService userService)
    {
        _mapService = mapService;
        _userService = userService;
    }

    private bool IsAdmin() =>
        HttpContext.Session.GetInt32("UserTypeId") is int t && t != 0;

    // GET /Admin/UserList?filter=donors|staff|inactive
    public async Task<IActionResult> UserList(string filter = null)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var sqlUsers = await _userService.GetAllUsersAsync();
        var userTypes = await _userService.GetAllUserTypesAsync();

        var mongoUsers = await _userService.GetAllUsersAsync(true);
        var mongoMap   = mongoUsers.GroupBy(m => m.UserId)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedOn).First());

        var userTypeDict = userTypes.ToDictionary(
            x => x.UserTypeId,
            x => x.UserTypeName
        );

        var list = sqlUsers.Select(u =>
        {
            mongoMap.TryGetValue(u.UserId, out var mongo);
            userTypeDict.TryGetValue(u.UserTypeId, out var typeName);

            return new AdminUserListViewModel
            {
                UserId       = u.UserId,
                Email        = u.Email,
                UserTypeId   = u.UserTypeId,
                UserTypeName = typeName ?? $"Type {u.UserTypeId}",
                Active       = u.Active,
                CreatedOn    = u.CreatedOn,
                Name         = mongo?.UserDetails?.Name ?? "—",
                BloodGroup   = mongo?.BloodGroup ?? "",
                Phone        = mongo?.UserDetails?.Phone ?? "",
                Verified     = mongo?.Verified ?? false
            };
        }).AsQueryable();

        list = filter switch
        {
            "donors"   => list.Where(u => u.UserTypeId == 0),
            "staff"    => list.Where(u => u.UserTypeId != 0),
            "inactive" => list.Where(u => !u.Active),
            _          => list
        };

        ViewBag.Filter     = filter ?? "all";
        ViewBag.TotalAll      = sqlUsers.Count;
        ViewBag.TotalDonors   = sqlUsers.Count(u => u.UserTypeId == 0);
        ViewBag.TotalStaff    = sqlUsers.Count(u => u.UserTypeId != 0);
        ViewBag.TotalInactive = sqlUsers.Count(u => !u.Active);

        return View(list.ToList());
    }

    // GET /Admin/CreateUser
    public async Task<IActionResult> CreateUser()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        return View(new AdminCreateUserViewModel
        {
            UserTypes = await _userService.GetAllUserTypesAsync()
        });
    }

    // POST /Admin/CreateUser
    [HttpPost]
    public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        if (await _userService.EmailExistsAsync(model.Email))
        {
            ViewBag.Error = "This email address is already registered.";
            model.UserTypes = await _userService.GetAllUserTypesAsync();
            return View(model);
        }

        await _userService.AdminCreateUserAsync(
            model.SelectedUserTypeId,
            model.Email,
            model.Password,
            model.Name,
            model.Phone,
            model.Gender,
            model.BloodGroup,
            model.Address,
            model.LocationText,
            model.AvailableLat,
            model.AvailableLng,
            model.NIC);

        TempData["Success"] = $"Account for '{model.Name}' created successfully.";
        return RedirectToAction("UserList");
    }

    // GET /Admin/EditUser/5
    public async Task<IActionResult> EditUser(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var sqlUser = await _userService.GetUserById(id,true);
        if (sqlUser == null) return NotFound();

        var mongo = await _userService.GetUserById(id);

        var vm = new UserViewModel
        {
            UserId     = sqlUser.UserId,
            UserTypeId = sqlUser.UserTypeId,
            Email      = sqlUser.Email,
            Active     = sqlUser.Active,
            Name       = mongo?.UserDetails?.Name,
            Address    = mongo?.UserDetails?.Address,
            NIC        = mongo?.UserDetails?.NIC,
            Phone      = mongo?.UserDetails?.Phone,
            DonatedLng  = mongo?.DonatedLocation?.Coordinates?[0] ?? 0,
            DonatedLat  = mongo?.DonatedLocation?.Coordinates?[1] ?? 0,
            AvailableLng = mongo?.AvailableLocation?.Coordinates?[0] ?? 0,
            AvailableLat = mongo?.AvailableLocation?.Coordinates?[1] ?? 0,
            LocationText = mongo?.LocationText,
            Concent      = mongo?.Concent ?? false,
            BloodGroup   = mongo?.BloodGroup ?? ""
        };

        ViewBag.UserTypes = _userService.GetAllUserTypesAsync();
        return View(vm);
    }

    // POST /Admin/EditUser
    [HttpPost]
    public async Task<IActionResult> EditUser(UserViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        await _userService.UpdateAsync(
            model.UserId, model.UserTypeId, model.Email, model.Active,
            model.Name, model.Address, model.NIC, model.Phone,
            model.DonatedLng, model.DonatedLat,
            model.AvailableLng, model.AvailableLat,
            model.LocationText, model.Concent, model.BloodGroup);

        TempData["Success"] = "User account updated successfully.";
        return RedirectToAction("UserList");
    }

    // POST /Admin/ToggleActive
    [HttpPost]
    public async Task<IActionResult> ToggleActive(int userId, bool currentActive)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        if (currentActive)
            await _userService.DeactivateUserAsync(userId);
        else
            await _userService.ReactivateUserAsync(userId);

        return RedirectToAction("UserList");
    }

    [HttpGet]
    public IActionResult Donate()
    {
        return View(new DonateViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> SearchUser(DonateViewModel model)
    {
        var user = await _userService.GetUserById(model.UserId);
       
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View("Donate", model);
        }

        model.Name = user.UserDetails.Name;
        model.BloodGroup = user.BloodGroup;
        model.UserFound = true;

        model.History = user.Donate_History?
            .OrderByDescending(x => x.Date)
            .ToList();

        model.Donation_Num = user.Donate_History?.Count + 1 ?? 1;

        return View("Donate", model);
    }

    [HttpPost]
    public async Task<IActionResult> Donate(DonateViewModel model)
    {
        var user = await _userService.GetUserById(model.UserId);

        if (user == null)
            return NotFound();

        var lastDonation = user.Donate_History?
            .OrderByDescending(x => x.Date)
            .FirstOrDefault();

        if (lastDonation != null)
        {
            var months = (DateTime.Now - lastDonation.Date).TotalDays;

            if (months < 120)
            {
                TempData["Error"] = "User cannot donate yet.";
                return RedirectToAction("Donate");
            }
        }

        var donation = new DonateHistory
        {
            Donation_Num = user.Donate_History?.Count + 1 ?? 1,
            Date = model.Date,
            Location = new DonateLocation
            {
                Lat = model.Lat,
                Lon = model.Lon
            }
        };


        await _userService.Donate(model.UserId,donation);
        TempData["Success"] = "Donation recorded successfully";

        return RedirectToAction("Donate");
    }
}
