using System;
using Microsoft.EntityFrameworkCore;
using RedConnect.Interfaces;
using RedConnect.Models;
using RedConnectApp.Models;

namespace RedConnectApp.DAL;

public class MSSQLDBContext : DbContext, IAppDbContext
{

    public MSSQLDBContext(DbContextOptions<MSSQLDBContext> options)
        : base(options)
    {
    }

    public DbSet<MsSqlUser> Users { get; set; }
    public DbSet<UserType> UserType { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit logging, timestamps, etc.
        return await base.SaveChangesAsync(cancellationToken);
    }

    

}
