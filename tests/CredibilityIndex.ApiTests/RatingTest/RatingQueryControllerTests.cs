using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Api.Contracts.Rating;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Application.Common;

namespace CredibilityIndex.ApiTests;

public class RatingQueryControllerTests
{
    private readonly Mock<IRatingQueryRepository> _mockQueryRepo;
    private readonly Mock<IWebsiteRepository> _mockWebsiteRepo;
    private readonly RatingQueryController _controller;

    public RatingQueryControllerTests()
    {
        _mockQueryRepo = new Mock<IRatingQueryRepository>();
        _mockWebsiteRepo = new Mock<IWebsiteRepository>();
        _controller = new RatingQueryController(_mockQueryRepo.Object, _mockWebsiteRepo.Object);
    }

    [Fact]
    public async Task GetWebsiteRatings_ReturnsNotFound_WhenInvalidDomain()
    {
        // Act
        var result = await _controller.GetWebsiteRatings("invalid domain!!!!");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetWebsiteRatings_ReturnsNotFound_WhenWebsiteDoesNotExist()
    {
        // Arrange
        _mockWebsiteRepo.Setup(r => r.GetByNormalizedDomainAsync("example.com"))
            .ReturnsAsync((Website)null);

        // Act
        var result = await _controller.GetWebsiteRatings("example.com");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetWebsiteRatings_ReturnsOk_WithPaginatedRatings()
    {
        // Arrange
        var website = new Website
        {
            Id = 1,
            Domain = "example.com",
            Name = "Example",
            DisplayName = "Example Site",
            CreatedAt = DateTime.UtcNow
        };

        var ratings = new List<RatingEntity>
        {
            new()
            {
                Id = 1,
                WebsiteId = 1,
                UserId = Guid.NewGuid(),
                Accuracy = 4,
                BiasNeutrality = 4,
                Transparency = 4,
                SafetyTrust = 4,
                Comment = "Good site",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 2,
                WebsiteId = 1,
                UserId = Guid.NewGuid(),
                Accuracy = 3,
                BiasNeutrality = 3,
                Transparency = 3,
                SafetyTrust = 3,
                Comment = "Average site",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _mockWebsiteRepo.Setup(r => r.GetByNormalizedDomainAsync("example.com"))
            .ReturnsAsync(website);

        _mockQueryRepo.Setup(r => r.GetPaginatedRatingsAsync(1, 1, 10))
            .ReturnsAsync((ratings, 2));

        // Act
        var result = await _controller.GetWebsiteRatings("example.com", 1, 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PaginatedRatingsResponse>().Subject;
        
        response.Domain.Should().Be("example.com");
        response.TotalCount.Should().Be(2);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetWebsiteRatings_ReturnsEmptyList_WhenNoRatingsExist()
    {
        // Arrange
        var website = new Website
        {
            Id = 2,
            Domain = "newsite.com",
            Name = "New Site",
            DisplayName = "New Site",
            CreatedAt = DateTime.UtcNow
        };

        _mockWebsiteRepo.Setup(r => r.GetByNormalizedDomainAsync("newsite.com"))
            .ReturnsAsync(website);

        _mockQueryRepo.Setup(r => r.GetPaginatedRatingsAsync(2, 1, 10))
            .ReturnsAsync((new List<RatingEntity>(), 0));

        // Act
        var result = await _controller.GetWebsiteRatings("newsite.com", 1, 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PaginatedRatingsResponse>().Subject;
        
        response.TotalCount.Should().Be(0);
        response.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWebsiteRatings_UsesPaginationParameters()
    {
        // Arrange
        var website = new Website
        {
            Id = 1,
            Domain = "example.com",
            Name = "Example",
            DisplayName = "Example Site",
            CreatedAt = DateTime.UtcNow
        };

        var ratings = new List<RatingEntity>();

        _mockWebsiteRepo.Setup(r => r.GetByNormalizedDomainAsync("example.com"))
            .ReturnsAsync(website);

        _mockQueryRepo.Setup(r => r.GetPaginatedRatingsAsync(1, 2, 20))
            .ReturnsAsync((ratings, 0));

        // Act
        var result = await _controller.GetWebsiteRatings("example.com", 2, 20);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PaginatedRatingsResponse>().Subject;
        
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(20);
        
        _mockQueryRepo.Verify(r => r.GetPaginatedRatingsAsync(1, 2, 20), Times.Once);
    }

    [Fact]
    public async Task GetWebsiteRatings_NormalizesUrlBeforeLookup()
    {
        // Arrange
        var website = new Website
        {
            Id = 1,
            Domain = "example.com",
            Name = "Example",
            DisplayName = "Example Site",
            CreatedAt = DateTime.UtcNow
        };

        _mockWebsiteRepo.Setup(r => r.GetByNormalizedDomainAsync("example.com"))
            .ReturnsAsync(website);

        _mockQueryRepo.Setup(r => r.GetPaginatedRatingsAsync(1, 1, 10))
            .ReturnsAsync((new List<RatingEntity>(), 0));

        // Act
        var result = await _controller.GetWebsiteRatings("https://example.com/some/path");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        _mockWebsiteRepo.Verify(r => r.GetByNormalizedDomainAsync("example.com"), Times.Once);
    }

    [Fact]
    public async Task GetWebsiteRatings_MapsRatingDataCorrectly()
    {
        // Arrange
        var website = new Website
        {
            Id = 1,
            Domain = "example.com",
            Name = "Example",
            DisplayName = "Example Site",
            CreatedAt = DateTime.UtcNow
        };

        var createdDate = DateTime.UtcNow.AddDays(-3);
        var rating = new RatingEntity
        {
            Id = 100,
            WebsiteId = 1,
            UserId = Guid.NewGuid(),
            Accuracy = 4,
            BiasNeutrality = 3,
            Transparency = 4,
            SafetyTrust = 4,
            Comment = "Excellent site!",
            CreatedAt = createdDate
        };

        _mockWebsiteRepo.Setup(r => r.GetByNormalizedDomainAsync("example.com"))
            .ReturnsAsync(website);

        _mockQueryRepo.Setup(r => r.GetPaginatedRatingsAsync(1, 1, 10))
            .ReturnsAsync((new List<RatingEntity> { rating }, 1));

        // Act
        var result = await _controller.GetWebsiteRatings("example.com");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PaginatedRatingsResponse>().Subject;
        
        response.Items.Should().HaveCount(1);
        var item = response.Items.First();
        item.Id.Should().Be(100);
        item.Accuracy.Should().Be(4);
        item.BiasNeutrality.Should().Be(3);
        item.Transparency.Should().Be(4);
        item.SafetyTrust.Should().Be(4);
        item.Comment.Should().Be("Excellent site!");
        item.CreatedAt.Should().Be(createdDate);
        item.DisplayName.Should().Be("Anonymous Reviewer");
    }
}
