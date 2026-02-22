using System;
using System.ComponentModel.DataAnnotations;

namespace RedConnect.Models;

public class MsSqlUser
{
    [Key]
    public int UserId { get; set; }
    public int UserTypeId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
    public DateTime LastUpdatedOn { get; set; } = DateTime.UtcNow;
}
