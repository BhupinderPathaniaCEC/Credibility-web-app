using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using static OpenIddict.Abstractions.OpenIddictConstants;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Api.Contracts.Rating;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Tests.Controllers
{
    public class RatingControllerTests
    {
        private readonly Mock<IRatingRepository> _mockRepo;
        private readonly RatingController _controller;

        public RatingControllerTests()
        {
            // 1. Create a "fake" repository
            _mockRepo = new Mock<IRatingRepository>();

            // 2. Pass the fake repository into the real controller
            _controller = new RatingController(_mockRepo.Object);
        }

        [Fact]
        public async Task SubmitRating_WhenUserIsNotLoggedIn_ReturnsUnauthorized()
        {
            // Arrange: Create a blank HttpContext with NO logged-in user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var request = new CreateRatingRequest { WebsiteId = Guid.NewGuid() };

            // Act: Try to submit the rating
            var result = await _controller.SubmitRating(request);

            // Assert: The API should block them with a 401 Unauthorized
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task SubmitRating_WhenValid_CallsUpsertAndReturnsOk()
        {
            // Arrange
            var testUserId = Guid.NewGuid();
            var testWebsiteId = Guid.NewGuid();
            var expectedAverageScore = 4.25;

            // 1. Fake the Logged-In User (Give them a valid JWT Subject Claim)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(Claims.Subject, testUserId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // 2. Create the incoming request DTO
            var request = new CreateRatingRequest
            {
                WebsiteId = testWebsiteId,
                Accuracy = 4,
                BiasNeutrality = 5,
                Transparency = 4,
                SafetyTrust = 4,
                Comment = "Great site!"
            };

            // 3. Tell the fake repository what to return when GetAverageCredibilityAsync is called
            _mockRepo.Setup(repo => repo.GetAverageCredibilityAsync(testWebsiteId))
                     .ReturnsAsync(expectedAverageScore);

            // Act
            var result = await _controller.SubmitRating(request);

            // Assert
            // 1. Verify it returns a 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // 2. Verify the payload matches our CreateRatingResponse DTO
            var response = Assert.IsType<CreateRatingResponse>(okResult.Value);
            Assert.Equal(testWebsiteId, response.WebsiteId);
            Assert.Equal(expectedAverageScore, response.AverageScore);

            // 3. THE MOST IMPORTANT TEST: Verify the Controller actually told the Repository to Upsert!
            _mockRepo.Verify(repo => repo.UpsertRatingAsync(It.Is<RatingEntity>(r => 
                r.UserId == testUserId && 
                r.WebsiteId == testWebsiteId &&
                r.Accuracy == 4)), 
                Times.Once); // Ensures it ran exactly 1 time
        }
    }
}