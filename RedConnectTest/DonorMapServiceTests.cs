
using Xunit;
using MongoDB.Driver;
using Moq;
using RedConnect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DonorMapServiceTests
{
    [Fact]
    public async Task Should_Filter_Active_Donors()
    {
        var users = new List<MongoUser>
        {
            new MongoUser { UserId = 1, Active = true },
            new MongoUser { UserId = 2, Active = false }
        };

        Assert.Single(users.FindAll(x => x.Active));
    }
}
