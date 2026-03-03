namespace RedConnect.ViewModels;

public class DashboardViewModel
{
    public long TotalDonors { get; set; }
    public long VerifiedDonors { get; set; }
    public long PendingDonors { get; set; }
    public long TotalBanks { get; set; }

    public string UserName { get; set; }
    public string BloodGroup { get; set; }
    public int UserTypeId { get; set; }
}
