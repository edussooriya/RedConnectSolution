
using Xunit;
using Microsoft.EntityFrameworkCore;
using RedConnectApp.DAL;
using RedConnect.Models;
using System.Threading.Tasks;

public class EfCoreInMemoryTests
{
    [Fact]
    public async Task Should_Add_And_Read_User()
    {
        var options = new DbContextOptionsBuilder<MSSQLDBContext>()
            .UseInMemoryDatabase("TestDB")
            .Options;

        using var context = new MSSQLDBContext(options);

        context.Users.Add(new MsSqlUser { Email = "test@test.com", Password = "123" });
        await context.SaveChangesAsync();

        var users = await context.Users.ToListAsync();

        Assert.Single(users);
    }
}
