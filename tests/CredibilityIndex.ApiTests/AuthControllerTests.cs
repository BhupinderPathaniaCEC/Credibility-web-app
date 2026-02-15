using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Infrastructure.Auth;

namespace CredibilityIndex.ApiTests
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserMgr;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserMgr = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            // This tests the Controller (Api Layer)
            _controller = new AuthController(_mockUserMgr.Object, null);
        }

        [Fact]
        public async Task Register_Returns200_WhenDataIsValid()
        {
            var request = new RegisterRequest { Email = "api@test.com", Password = "Password123!" };
            _mockUserMgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(request);

            result.Should().BeOfType<OkObjectResult>();
        }
        
        [Fact]
        public async Task Register_ReturnsError_WhenEmailAlreadyExists()
        {
            // ARRANGE
            var request = new RegisterRequest { Email = "exists@test.com", Password = "Password123!" };
            var identityError = new IdentityError { Code = "Duplicate", Description = "Email taken" };
            
            // Tell the fake manager to say "No, this email is taken!"
            _mockUserMgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Failed(identityError));

            // ACT
            var result = await _controller.Register(request);

            // ASSERT: Check if it returns a 'ValidationProblem' (400 Bad Request)
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}