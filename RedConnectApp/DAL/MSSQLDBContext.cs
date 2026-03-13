using Microsoft.EntityFrameworkCore;
using RedConnect.Models;
using RedConnectApp.Models;

namespace RedConnectApp.DAL;

public class MSSQLDBContext : DbContext
{
    public MSSQLDBContext(DbContextOptions<MSSQLDBContext> options) : base(options) { }
    public DbSet<MsSqlUser> Users { get; set; }
    public DbSet<UserType> UserType { get; set; }

    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
}
