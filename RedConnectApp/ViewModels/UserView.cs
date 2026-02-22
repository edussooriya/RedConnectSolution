namespace RedConnect.ViewModels;

public class UserViewModel
{
    public int UserId { get; set; }
    public int UserTypeId { get; set; }
    public string Email { get; set; }
    public bool Active { get; set; }

    // Mongo Fields
    public string Name { get; set; }
    public string Address { get; set; }
    public string NIC { get; set; }

    public string Phone { get; set; }

    public double DonatedLng { get; set; }
    public double DonatedLat { get; set; }

    public double AvailableLng { get; set; }
    public double AvailableLat { get; set; }

    public string LocationText { get; set; }

    public string BloodGroup { get; set; }
    public bool Concent { get; set; }
}