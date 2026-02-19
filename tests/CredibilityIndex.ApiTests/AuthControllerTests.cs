using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Api.Contracts;
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
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new[] { new Mock<IUserValidator<ApplicationUser>>().Object };
            var passwordValidators = new[] { new Mock<IPasswordValidator<ApplicationUser>>().Object };
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var optionsAccessor = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();

            _mockUserMgr = new Mock<UserManager<ApplicationUser>>(
                store.Object, optionsAccessor.Object, passwordHasher.Object, userValidators, passwordValidators, 
                keyNormalizer.Object, errors.Object, contextAccessor.Object, logger.Object);

            var contextAccessor2 = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInLogger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
            var authenticationSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
            var confirmationService = new Mock<IUserConfirmation<ApplicationUser>>();

            var mockSignInMgr = new Mock<SignInManager<ApplicationUser>>(_mockUserMgr.Object, contextAccessor2.Object, 
                claimsFactory.Object, new Mock<IdentityOptions>().Object, signInLogger.Object, authenticationSchemeProvider.Object, confirmationService.Object);
            var mockLogger = new Mock<ILogger<AuthController>>();

            // This tests the Controller (Api Layer)
            _controller = new AuthController(_mockUserMgr.Object, mockSignInMgr.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Register_Returns200_WhenDataIsValid()
        {
            var request = new RegisterRequest("api@test.com", "Password123!", "Test User");
            _mockUserMgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(request);

            result.Should().BeOfType<OkObjectResult>();
        }
        
        [Fact]
        public async Task Register_ReturnsError_WhenEmailAlreadyExists()
        {
            // ARRANGE
            var request = new RegisterRequest("exists@test.com", "Password123!", "Test User");
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