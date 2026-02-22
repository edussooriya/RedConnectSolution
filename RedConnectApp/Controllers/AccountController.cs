using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using RedConnect.DAL;
using RedConnect.Models;
using RedConnect.ViewModels;

namespace RedConnect.Controllers;

public class AccountController : Controller
{
    private readonly UserRepository _repo;

    public AccountController(UserRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register( string email, string password,
        string name, string address, string nic,
        double donatedLng, double donatedLat,
        double availableLng, double availableLat, string LocationText,string phone , int userTypeId =0)
    {
        //default userTypeId = 0 means a donor

        await _repo.RegisterAsync(userTypeId, email, password,
            name, address, nic,
            donatedLng, donatedLat,
            availableLng, availableLat, LocationText,phone);

        return RedirectToAction("Login");
    }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _repo.LoginAsync(email, password);

        if (user == null)
        {
            ViewBag.Error = "Invalid credentials";
            return View();
        }
        return RedirectToAction("Edit", new { id = user.UserId });
       ;
    }

    public async Task<IActionResult> Edit(int id)
    {
        var sqlUser = await _repo.GetByIdAsync(id);

        if (sqlUser == null)
            return NotFound();

        var mongoUser = await _repo.GetMongoUserAsync(id);

        var model = new UserViewModel
        {
            UserId = sqlUser.UserId,
            UserTypeId = sqlUser.UserTypeId,
            Email = sqlUser.Email,
            Active = sqlUser.Active,

            Name = mongoUser?.UserDetails?.Name,
            Address = mongoUser?.UserDetails?.Address,
            NIC = mongoUser?.UserDetails?.NIC,
            Phone = mongoUser?.UserDetails?.Phone,

            DonatedLng = mongoUser?.DonatedLocation?.Coordinates?[0] ?? 0,
            DonatedLat = mongoUser?.DonatedLocation?.Coordinates?[1] ?? 0,

            AvailableLng = mongoUser?.AvailableLocation?.Coordinates?[0] ?? 0,
            AvailableLat = mongoUser?.AvailableLocation?.Coordinates?[1] ?? 0,

            LocationText = mongoUser?.LocationText,
            Concent = mongoUser?.Concent ?? false,
            BloodGroup = mongoUser?.BloodGroup ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserViewModel model)
    {
        await _repo.UpdateAsync(
            model.UserId,
            model.UserTypeId,
            model.Email,
            model.Active,
            model.Name,
            model.Address,
            model.NIC,
            model.Phone,
            model.DonatedLng,
            model.DonatedLat,
            model.AvailableLng,
            model.AvailableLat,model.LocationText, model.Concent, model.BloodGroup);

        return RedirectToAction("Edit", new { id = model.UserId });
    }
}
