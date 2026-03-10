using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CredibilityIndex.Api.Controllers;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CredibilityIndex.ApiTests;

public class TestControllerTests
{
    private readonly TestController _controller;

    public TestControllerTests()
    {
        _controller = new TestController();
    }

    private void SetupControllerWithUser(string userId = "user-123", string email = "user@test.com", string name = "Test User")
    {
        // Setup the controller's User principal with necessary claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext { User = principal };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }

    private void SetupControllerWithoutUser()
    {
        var context = new DefaultHttpContext { User = new ClaimsPrincipal() };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }

    // --- GET SECURE DATA TESTS ---
    [Fact]
    public void GetSecureData_ReturnsOk_WhenUserIsAuthenticated()
    {
        // Arrange
        SetupControllerWithUser("user-123", "testuser@example.com", "Test User");

        // Act
        var result = _controller.GetSecureData();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        okResult.Value.ToString().Should().Contain("This is protected information!");
    }

    [Fact]
    public void GetSecureData_IncludesUserInfo_InResponse()
    {
        // Arrange
        SetupControllerWithUser("user-456", "protected@test.com", "Protected User");

        // Act
        var result = _controller.GetSecureData();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.ToString().Should().Contain("Protected User");
        okResult.Value.ToString().Should().Contain("timestamp");
    }

    [Fact]
    public void GetSecureData_IncludesMessage_InResponse()
    {
        // Arrange
        SetupControllerWithUser();

        // Act
        var result = _controller.GetSecureData();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.ToString().Should().Contain("message");
    }

    [Fact]
    public void GetSecureData_IncludesUtcNowTimestamp()
    {
        // Arrange
        SetupControllerWithUser();
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = _controller.GetSecureData();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.ToString().Should().Contain("timestamp");
    }

    [Fact]
    public void GetSecureData_WorksWithDifferentUsers()
    {
        // Arrange - User 1
        SetupControllerWithUser("user-1", "user1@test.com");

        // Act
        var result1 = _controller.GetSecureData();

        // Assert
        var okResult1 = result1.Should().BeOfType<OkObjectResult>().Subject;
        okResult1.Value.Should().NotBeNull();

        // Arrange - User 2
        SetupControllerWithUser("user-2", "user2@test.com");

        // Act
        var result2 = _controller.GetSecureData();

        // Assert
        var okResult2 = result2.Should().BeOfType<OkObjectResult>().Subject;
        okResult2.Value.Should().NotBeNull();
    }
}
