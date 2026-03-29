using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using RedConnect.Controllers;
using RedConnect.Interfaces;
using RedConnect.Models;
using System.Collections.Generic;


public class AdminControllerTests
{
    [Fact]
    public async Task UserList_ReturnsView()
    {
        var userService = new Mock<IUserService>();

        //Mock UserTypes
        userService.Setup(x => x.GetAllUserTypesAsync())
            .ReturnsAsync(new List<UserType>
            {
            new UserType { UserTypeId = 1, UserTypeName = "Admin" },
            new UserType { UserTypeId = 0, UserTypeName = "Donor" }
            });

        //Mock SQL users
        userService.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(new List<MsSqlUser>());

        //Mock Mongo users
        userService.Setup(x => x.GetAllUsersAsync(true))
            .ReturnsAsync(new List<MongoUser>());

        var donorMapService = new DonorMapService(null);

        var controller = new AdminController(
            donorMapService,
            userService.Object
        );

        // Mock session (admin)
        var session = new Mock<ISession>();
        int userTypeId = 1;
        var bytes = BitConverter.GetBytes(userTypeId);

        session.Setup(s => s.TryGetValue("UserTypeId", out bytes))
               .Returns(true);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session.Object;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.UserList(null);

        Assert.IsType<ViewResult>(result);
    }
}