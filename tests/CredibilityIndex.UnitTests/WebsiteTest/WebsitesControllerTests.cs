using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Api.Contracts.Website;

namespace CredibilityIndex.ApiTests
{
    public class WebsitesControllerTests
    {
        private readonly Mock<IWebsiteRepository> _mockRepo;
        private readonly WebsitesController _controller;

        public WebsitesControllerTests()
        {
            _mockRepo = new Mock<IWebsiteRepository>();
            _controller = new WebsitesController(_mockRepo.Object);
        }

        // --- SEARCH TESTS ---
        [Fact]
        public async Task Search_ReturnsOk_WithWebsites_WhenQueryProvided()
        {
            // Arrange
            var websites = new List<Website>
            {
                new()
                {
                    Id = 1,
                    Name = "Example News",
                    Domain = "example.com",
                    DisplayName = "Example News",
                    Description = "Example news website",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Category = new Category { Id = 1, Name = "News", Slug = "news" }
                },
                new()
                {
                    Id = 2,
                    Name = "Example Tech",
                    Domain = "exampletech.com",
                    DisplayName = "Example Tech",
                    Description = "Tech site",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Category = new Category { Id = 2, Name = "Technology", Slug = "technology" }
                }
            };

            _mockRepo.Setup(r => r.SearchWebsitesAsync("example"))
                     .ReturnsAsync(websites);

            // Act
            var result = await _controller.Search("example");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            _mockRepo.Verify(r => r.SearchWebsitesAsync("example"), Times.Once);
        }

        [Fact]
        public async Task Search_ReturnsOk_WithEmptyList_WhenNoResultsFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SearchWebsitesAsync("nonexistent"))
                     .ReturnsAsync(new List<Website>());

            // Act
            var result = await _controller.Search("nonexistent");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var enumerable = okResult.Value as System.Collections.IEnumerable;
            enumerable.Should().NotBeNull();
        }

        [Fact]
        public async Task Search_ReturnsOk_WithAllWebsites_WhenQueryIsNull()
        {
            // Arrange
            var websites = new List<Website>
            {
                new()
                {
                    Id = 1,
                    Name = "Site One",
                    Domain = "site1.com",
                    DisplayName = "Site One",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Category = new Category { Id = 1, Name = "News", Slug = "news" }
                }
            };

            _mockRepo.Setup(r => r.SearchWebsitesAsync(null))
                     .ReturnsAsync(websites);

            // Act
            var result = await _controller.Search(null);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        // --- GET WEBSITE DETAILS TESTS ---
        [Fact]
        public async Task GetWebsiteDetails_ReturnsOk_WithWebsiteAndSnapshot()
        {
            // Arrange
            var snapshot = new CredibilitySnapshot
            {
                Score = 85,
                AvgAccuracy = 4.5,
                AvgBiasNeutrality = 4.0,
                AvgTransparency = 4.2,
                AvgSafetyTrust = 4.1,
                RatingCount = 20,
                ComputedAt = DateTime.UtcNow
            };

            var website = new Website
            {
                Id = 1,
                Name = "Example News",
                Domain = "example.com",
                DisplayName = "Example News Site",
                Description = "A credible news source",
                IsActive = true,
                Category = new Category { Id = 1, Name = "News", Slug = "news" },
                CredibilitySnapshot = snapshot
            };

            _mockRepo.Setup(r => r.GetWebsiteWithSnapshotByDomainAsync("example.com"))
                     .ReturnsAsync(website);

            // Act
            var result = await _controller.GetWebsiteDetails("example.com");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            var response = okResult.Value.Should().BeOfType<WebsiteDetailResponse>().Subject;
            response.Domain.Should().Be("example.com");
            response.Snapshot.Should().NotBeNull();
            response.Snapshot.Score.Should().Be(85);
        }

        [Fact]
        public async Task GetWebsiteDetails_ReturnsOk_WithWebsiteButNoSnapshot()
        {
            // Arrange
            var website = new Website
            {
                Id = 2,
                Name = "New Site",
                Domain = "newsite.com",
                DisplayName = "New Site",
                Description = "A new website",
                IsActive = true,
                Category = new Category { Id = 1, Name = "General", Slug = "general" },
                CredibilitySnapshot = null
            };

            _mockRepo.Setup(r => r.GetWebsiteWithSnapshotByDomainAsync("newsite.com"))
                     .ReturnsAsync(website);

            // Act
            var result = await _controller.GetWebsiteDetails("newsite.com");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<WebsiteDetailResponse>().Subject;
            response.Snapshot.Should().BeNull();
        }

        [Fact]
        public async Task GetWebsiteDetails_ReturnsBadRequest_WhenDomainParameterEmpty()
        {
            // Act
            var result = await _controller.GetWebsiteDetails("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetWebsiteDetails_ReturnsBadRequest_WhenDomainParameterIsNull()
        {
            // Act
            var result = await _controller.GetWebsiteDetails(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetWebsiteDetails_ReturnsBadRequest_WhenDomainParameterIsWhitespace()
        {
            // Act
            var result = await _controller.GetWebsiteDetails("   ");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetWebsiteDetails_ReturnsNotFound_WhenWebsiteDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetWebsiteWithSnapshotByDomainAsync("nonexistent.com"))
                     .ReturnsAsync((Website)null);

            // Act
            var result = await _controller.GetWebsiteDetails("nonexistent.com");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetWebsiteDetails_DecodesUrlEncodedDomain()
        {
            // Arrange
            var website = new Website
            {
                Id = 3,
                Name = "Example",
                Domain = "example.com",
                DisplayName = "Example",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Category = new Category { Id = 1, Name = "Test", Slug = "test" },
                CredibilitySnapshot = null
            };

            _mockRepo.Setup(r => r.GetWebsiteWithSnapshotByDomainAsync("example.com"))
                     .ReturnsAsync(website);

            // Act - Pass URL-encoded domain
            var result = await _controller.GetWebsiteDetails("example%2Ecom");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            _mockRepo.Verify(r => r.GetWebsiteWithSnapshotByDomainAsync("example.com"), Times.Once);
        }
    }
}