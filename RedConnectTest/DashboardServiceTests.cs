
using Xunit;
using Moq;
using RedConnect.Services;
using RedConnect.Interfaces;
using System.Threading.Tasks;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardStats_ReturnsValidTuple()
    {
        var mongoRepo = new Mock<IMongoRepository>();
        var sqlContext = new Mock<IAppDbContext>();

        var service = new DashboardService(mongoRepo.Object, sqlContext.Object);

        var result = await service.GetDashboardStatsAsync();

        Assert.True(result.TotalDonors >= 0);
    }
}
