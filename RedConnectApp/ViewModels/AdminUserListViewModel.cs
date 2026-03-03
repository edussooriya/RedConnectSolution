namespace RedConnect.ViewModels;

public class AdminUserListViewModel
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int UserTypeId { get; set; }
    public string UserTypeName { get; set; }
    public bool Active { get; set; }
    public bool Verified { get; set; }
    public string BloodGroup { get; set; }
    public string Phone { get; set; }
    public DateTime CreatedOn { get; set; }
}
