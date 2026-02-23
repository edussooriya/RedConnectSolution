using RedConnect.Models;

namespace RedConnect.ViewModels
{
    public class BloodBankViewModel
    {
        public string LocationName { get; set; }
        public string Address { get; set; }

        public string StaffEmail { get; set; }
        public string Password { get; set; }

        public int SelectedUserTypeId { get; set; }

        public List<UserType> UserTypes { get; set; } = new();
    }
}