using RedConnectApp.Models;

namespace RedConnectApp.ViewModels
{
    public class DonateViewModel
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string BloodGroup { get; set; }

        public DateTime Date { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public int Donation_Num { get; set; }

        public bool UserFound { get; set; }

        public List<DonateHistory> History { get; set; }

        public string LocationText { get; set; }
        public double DonatedLat { get; set; }
        public double DonatedLng { get; set; }
    }
}
