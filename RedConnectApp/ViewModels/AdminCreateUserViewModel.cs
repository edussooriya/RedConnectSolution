using RedConnect.Models;
using RedConnectApp.Enums;

namespace RedConnect.ViewModels;

public class AdminCreateUserViewModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string BloodGroup { get; set; }
    public GenderEnum Gender { get; set; }
    public int SelectedUserTypeId { get; set; }
    public List<UserType> UserTypes { get; set; } = new();

    // Donor-specific fields
    public string NIC { get; set; }
    public string Address { get; set; }
    public string LocationText { get; set; }
    public double AvailableLat { get; set; }
    public double AvailableLng { get; set; }
}
