using Microsoft.EntityFrameworkCore;
using RedConnect.Models;

namespace RedConnect.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<MsSqlUser> Users { get; set; }
}
