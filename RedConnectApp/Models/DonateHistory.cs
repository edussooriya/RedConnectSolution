namespace RedConnectApp.Models
{
    public class DonateHistory
    {
        public int Donation_Num { get; set; }

        public DateTime Date { get; set; }

        public DonateLocation Location { get; set; }
    }
}
