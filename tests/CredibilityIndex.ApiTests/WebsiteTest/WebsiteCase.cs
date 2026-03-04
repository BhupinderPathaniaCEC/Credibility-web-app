using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CredibilityIndex.Infrastructure.Tests;

public class WebsiteRepositoryTests : IDisposable
{
    private readonly CredibilityDbContext _context;
    private readonly WebsiteRepository _repository;

    public WebsiteRepositoryTests()
    {
        // 1. Setup a fresh In-Memory Database for each test
        var options = new DbContextOptionsBuilder<CredibilityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CredibilityDbContext(options);
        _repository = new WebsiteRepository(_context);

        // 2. Seed test data
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var category = new Category { Id = 1, Name = "News", Slug = "news" };
        _context.Categories.Add(category);

        _context.Websites.AddRange(
            new Website { Id = 1, Name = "BBC News", Domain = "bbc.com", CategoryId = 1 },
            new Website { Id = 2, Name = "CNN", Domain = "cnn.com", CategoryId = 1 },
            new Website { Id = 3, Name = "World Health", Domain = "who.int", CategoryId = 1 }
        );
        _context.SaveChanges();
    }

    // Cleanup after each test runs
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task SearchWebsitesAsync_ValidDomain_ReturnsMatch()
    {
        // Act
        var results = await _repository.SearchWebsitesAsync("bbc.com");

        // Assert
        Assert.Single(results);
        Assert.Equal("BBC News", results.First().Name);
        Assert.NotNull(results.First().Category); // Proves metadata is included
    }

    [Fact]
    public async Task SearchWebsitesAsync_PartialName_ReturnsMatch()
    {
        // Act
        var results = await _repository.SearchWebsitesAsync("Health");

        // Assert
        Assert.Single(results);
        Assert.Equal("who.int", results.First().Domain);
    }

    [Fact]
    public async Task SearchWebsitesAsync_MessyUrlInput_NormalizesAndReturnsMatch()
    {
        // Arrange - A user pasting a terrible URL
        var messyInput = "  https://WWW.CNN.COM/world/news/  ";

        // Act
        var results = await _repository.SearchWebsitesAsync(messyInput);

        // Assert
        Assert.Single(results);
        Assert.Equal("cnn.com", results.First().Domain);
    }

    [Fact]
    public async Task SearchWebsitesAsync_EmptyOrNullQuery_ReturnsEmptyList()
    {
        // Act
        var nullResults = await _repository.SearchWebsitesAsync(null);
        var emptyResults = await _repository.SearchWebsitesAsync("   ");

        // Assert
        Assert.Empty(nullResults);
        Assert.Empty(emptyResults);
    }

    [Fact]
    public async Task SearchWebsitesAsync_NoMatch_ReturnsEmptyList()
    {
        // Act
        var results = await _repository.SearchWebsitesAsync("fakewebsite.com");

        // Assert
        Assert.Empty(results);
    }
}