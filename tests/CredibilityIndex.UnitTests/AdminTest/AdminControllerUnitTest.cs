using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Infrastructure.Auth;
using CredibilityIndex.Application.Interfaces;

namespace CredibilityIndex.ApiTests;

public class AdminControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserMgr;
    private readonly Mock<IWebsiteRepository> _mockWebsiteRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        // 1. Setup the same "Hollow Shell" for UserManager
        _mockUserMgr = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, 
            null, null, null, null, null, null, null, null);

        _mockWebsiteRepo = new Mock<IWebsiteRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();

        _controller = new AdminController(_mockUserMgr.Object, _mockWebsiteRepo.Object, _mockCategoryRepo.Object);
    }

    [Fact]
    public void GetSystemStats_ReturnsOk_WithSystemInfo()
    {
        // ARRANGE: Create fake users
        var fakeUsers = new List<ApplicationUser>
        {
            new() { Id = "1", Email = "admin@test.com", UserName = "admin" },
            new() { Id = "2", Email = "user@test.com", UserName = "user" },
            new() { Id = "3", Email = "another@test.com", UserName = "another" }
        }.AsQueryable();

        _mockUserMgr.Setup(x => x.Users).Returns(fakeUsers);

        // ACT
        var result = _controller.GetSystemStats();

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        okResult.Value.ToString().Should().Contain("TotalUsers");
        okResult.Value.ToString().Should().Contain("ServerStatus");
        okResult.Value.ToString().Should().Contain("Healthy");
        okResult.Value.ToString().Should().Contain("LastBackup");
    }

    [Fact]
    public void GetAllUsers_ReturnsOk_WithUserList()
    {
        // ARRANGE: Create a fake list of users
        var fakeUsers = new List<ApplicationUser>
        {
            new() { Id = "1", Email = "admin@test.com", UserName = "admin" },
            new() { Id = "2", Email = "user@test.com", UserName = "user" }
        }.AsQueryable(); // Convert to Queryable so the Mock accepts it

        // Tell the mock: "When the controller accesses .Users, give it my fake list"
        _mockUserMgr.Setup(x => x.Users).Returns(fakeUsers);

        // ACT
        var result = _controller.GetAllUsers();

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        
        // We check if we actually got 2 users back
        okResult.Value.ToString().Should().Contain("TotalCount = 2");
    }
}