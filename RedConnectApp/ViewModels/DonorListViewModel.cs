namespace RedConnect.ViewModels
{
    public class DonorListViewModel
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? BloodGroup { get; set; }
        public string? LocationText { get; set; }
        public bool Verified { get; set; }
        public bool DocumentsUploaded { get; set; }
        public int ReportCount    { get; set; }
        public int RejectedCount  { get; set; }
        public int ApprovedCount  { get; set; }
    }
}
