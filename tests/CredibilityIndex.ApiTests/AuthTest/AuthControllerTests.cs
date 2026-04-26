using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Api.Contracts.Auth;
using CredibilityIndex.Infrastructure.Auth;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CredibilityIndex.ApiTests
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserMgr;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInMgr;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            // 1. MINIMAL USERMANAGER MOCK
            _mockUserMgr = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, 
            null, null, null, null, null, null, null, null);

            // 2. MINIMAL SIGNINMANAGER MOCK
            _mockSignInMgr = new Mock<SignInManager<ApplicationUser>>(
            _mockUserMgr.Object,
            new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null, null, null, null);

            _mockLogger = new Mock<ILogger<AuthController>>();
            
            _controller = new AuthController(_mockUserMgr.Object, _mockSignInMgr.Object, _mockLogger.Object);
        }

        private void SetupControllerWithUser(string userId = "user-123", string email = "user@test.com")
        {
            // Setup the controller's User principal with necessary claims
            var claims = new List<Claim>
            {
                new(Claims.Subject, userId),
                new(Claims.Email, email),
                new(Claims.Name, "User Name"),
                new(Claims.Role, "User"),
                new(Claims.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext { User = principal };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        // --- REGISTER TESTS ---
        [Fact]
        public async Task Register_ReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var request = new RegisterRequest("api@test.com", "Password123!", "Test User");
            _mockUserMgr.Setup(x => x.FindByEmailAsync(request.Email))
                        .ReturnsAsync((ApplicationUser)null);
            _mockUserMgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<RegisterResponse>();
        }
        
        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new RegisterRequest("exists@test.com", "Password123!", "Test User");
            
            _mockUserMgr.Setup(x => x.FindByEmailAsync(request.Email))
                        .ReturnsAsync(new ApplicationUser { Email = "exists@test.com" });

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Register_ReturnsValidationProblem_WhenCreateFails()
        {
            // Arrange
            var request = new RegisterRequest("new@test.com", "weak", "Test User");
            var identityErrors = new[] { new IdentityError { Code = "PasswordTooShort", Description = "Password too short" } };
            
            _mockUserMgr.Setup(x => x.FindByEmailAsync(request.Email))
                        .ReturnsAsync((ApplicationUser)null);
            _mockUserMgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
        }

        // --- GET CURRENT USER TESTS ---
        [Fact]
        public void GetCurrentUser_ReturnsOk_WithUserInfo()
        {
            // Arrange
            SetupControllerWithUser("user-123", "user@test.com");

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            okResult.Value.ToString().Should().Contain("user-123");
            okResult.Value.ToString().Should().Contain("user@test.com");
        }

        [Fact]
        public void GetCurrentUser_ReturnsOk_WithRoles()
        {
            // Arrange
            SetupControllerWithUser("user-123", "admin@test.com");

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Check that the response contains role information
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            json.Should().Contain("User");
            json.Should().Contain("Admin");
        }

        [Fact]
        public void GetCurrentUser_IncludesNameAndEmail()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(Claims.Subject, "user-456"),
                new(Claims.Email, "testuser@example.com"),
                new(Claims.Name, "Test User"),
                new(Claims.GivenName, "Test User Display")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };
            
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.ToString().Should().Contain("testuser@example.com");
            okResult.Value.ToString().Should().Contain("Test User");
        }
    }
}