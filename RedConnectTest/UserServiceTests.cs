
using Xunit;
using Moq;
using RedConnect.Interfaces;
using RedConnect.Services;
using RedConnect.Models;
using System.Threading.Tasks;

public class UserServiceTests
{
    [Fact]
    public async Task GetUserById_ReturnsNull_WhenUserNotFound()
    {
        var mockRepo = new Mock<IMongoRepository>();
        var mockContext = new Mock<IAppDbContext>();

        mockRepo.Setup(r => r.GetById(It.IsAny<int>()))
                .ReturnsAsync((MongoUser?)null);

        var service = new UserService(mockRepo.Object, mockContext.Object);

        var result = await service.GetUserById(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserById_ReturnsUser_WhenExists()
    {
        var mockRepo = new Mock<IMongoRepository>();
        var mockContext = new Mock<IAppDbContext>();

        mockRepo.Setup(r => r.GetById(1))
                .ReturnsAsync(new MongoUser { UserId = 1 });

        var service = new UserService(
            mockRepo.Object,
            mockContext.Object
        );

        var result = await service.GetUserById(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
    }
}
