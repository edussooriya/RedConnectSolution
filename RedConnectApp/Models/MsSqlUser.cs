using System;
using System.ComponentModel.DataAnnotations;

namespace RedConnect.Models;

public class MsSqlUser
{
    [Key]
    public int UserId { get; set; }
    public int UserTypeId { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character"
    )]
    public string Password { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
    public DateTime LastUpdatedOn { get; set; } = DateTime.UtcNow;
}
