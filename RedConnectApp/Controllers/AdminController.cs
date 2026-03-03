using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedConnect.DAL;
using RedConnect.ViewModels;
using RedConnectApp.DAL;

namespace RedConnect.Controllers;

public class AdminController : Controller
{
    private readonly MongoRepository _repo;
    private readonly MSSQLDBContext  _context;

    public AdminController(MongoRepository repo, MSSQLDBContext context)
    {
        _repo    = repo;
        _context = context;
    }

    private bool IsAdmin() =>
        HttpContext.Session.GetInt32("UserTypeId") is int t && t != 0;

    // GET /Admin/UserList?filter=donors|staff|inactive
    public async Task<IActionResult> UserList(string filter = null)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var sqlUsers  = await _context.Users.ToListAsync();
        var userTypes = await _context.UserType
            .ToDictionaryAsync(t => t.UserTypeId, t => t.UserTypeName);

        var mongoUsers = await _repo.GetAllMongoUsersAsync();
        var mongoMap   = mongoUsers.ToDictionary(m => m.UserId, m => m);

        var list = sqlUsers.Select(u =>
        {
            mongoMap.TryGetValue(u.UserId, out var mongo);
            userTypes.TryGetValue(u.UserTypeId, out var typeName);

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
    public IActionResult CreateUser()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        return View(new AdminCreateUserViewModel
        {
            UserTypes = _context.UserType.ToList()
        });
    }

    // POST /Admin/CreateUser
    [HttpPost]
    public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        if (await _repo.EmailExistsAsync(model.Email))
        {
            ViewBag.Error = "This email address is already registered.";
            model.UserTypes = _context.UserType.ToList();
            return View(model);
        }

        await _repo.AdminCreateUserAsync(
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

        var sqlUser = await _context.Users.FindAsync(id);
        if (sqlUser == null) return NotFound();

        var mongo = await _repo.GetMongoUserAsync(id);

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

        ViewBag.UserTypes = _context.UserType.ToList();
        return View(vm);
    }

    // POST /Admin/EditUser
    [HttpPost]
    public async Task<IActionResult> EditUser(UserViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        await _repo.UpdateAsync(
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
            await _repo.DeactivateUserAsync(userId);
        else
            await _repo.ReactivateUserAsync(userId);

        return RedirectToAction("UserList");
    }
}
