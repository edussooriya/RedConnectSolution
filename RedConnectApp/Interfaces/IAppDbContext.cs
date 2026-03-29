using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RedConnect.Models;
using RedConnectApp.Models;

namespace RedConnect.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<MsSqlUser> Users { get; }
        DbSet<UserType?> UserType { get; }
        DbSet<PasswordResetToken> PasswordResetTokens { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        EntityEntry Entry(object entity);



       
        
    }
}