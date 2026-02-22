namespace RedConnect.ViewModels
{
    public class DonorListViewModel
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? BloodGroup { get; set; }
        public string? LocationText { get; set; }
        public bool Verified { get; set; }
    }
}
