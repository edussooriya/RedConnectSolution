namespace RedConnect.ViewModels

{
    public class DonorMapViewModel
    {
        public int UserId { get; set; }

        public double Lat { get; set; }   // ← MUST exist
        public double Lng { get; set; }   // ← MUST exist

        public string Name { get; set; }

        public string Phone { get; set; }

        public bool Concent { get; set; }

        public string BloodGroup { get; set; }

        public string? LocationText { get; set; }

        public bool Verified { get; set; }
    }
}
