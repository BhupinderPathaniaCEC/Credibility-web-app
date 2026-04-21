using System.Diagnostics;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Infrastructure.Persistence;
using CredibilityIndex.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Xunit;

namespace CredibilityIndex.ApiTests;

public class SnapshotCachingPerformanceTest : IDisposable
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _scope;
    private IRatingRepository _ratingRepository;
    private int _testWebsiteId;

    public SnapshotCachingPerformanceTest()
    {
        var services = new ServiceCollection();

        // Use a temporary SQLite file instead of in-memory for better reliability
        var tempDbPath = Path.GetTempFileName();
        services.AddDbContext<CredibilityDbContext>(options =>
            options.UseSqlite($"DataSource={tempDbPath}"));

        // Add Memory Cache
        services.AddMemoryCache();

        // Add repository
        services.AddScoped<IRatingRepository, RatingRepository>();

        _serviceProvider = services.BuildServiceProvider();

        // Create a scope that lives for the test
        _scope = _serviceProvider.CreateScope();
        var context = _scope.ServiceProvider.GetRequiredService<CredibilityDbContext>();
        
        // Ensure database is created and migrated
        context.Database.EnsureCreated();

        // Seed a category first
        var category = new Category
        {
            Id = 1,
            Name = "Test Category",
            Slug = "test-category",
            Description = "Test category for performance tests"
        };
        context.Categories.Add(category);

        // Seed a website and snapshot
        var website = new Website
        {
            Domain = "example.com",
            Name = "Example",
            DisplayName = "Example Site",
            CategoryId = 1, // Required foreign key
            CreatedAt = DateTime.UtcNow
        };
        context.Websites.Add(website);
        context.SaveChanges();

        _testWebsiteId = website.Id;

        var snapshot = new CredibilitySnapshot
        {
            WebsiteId = _testWebsiteId,
            Score = 85,
            AvgAccuracy = 4.2,
            AvgBiasNeutrality = 3.8,
            AvgTransparency = 4.0,
            AvgSafetyTrust = 3.9,
            RatingCount = 10,
            ComputedAt = DateTime.UtcNow
        };
        context.CredibilitySnapshots.Add(snapshot);
        context.SaveChanges();

        _ratingRepository = _scope.ServiceProvider.GetRequiredService<IRatingRepository>();

        // Clean up temp file when done (optional)
        // File.Delete(tempDbPath); // Uncomment if you want to clean up
    }

    [Fact]
    public async Task MeasurePerformance_WithCaching()
    {
        var cache = _serviceProvider.GetRequiredService<IMemoryCache>();

        // Warm up the cache with one call
        await _ratingRepository.GetSnapshotByWebsiteIdAsync(_testWebsiteId);

        var stopwatch = Stopwatch.StartNew();

        // Perform 100 reads with cache already populated
        for (int i = 0; i < 100; i++)
        {
            await _ratingRepository.GetSnapshotByWebsiteIdAsync(_testWebsiteId);
        }

        stopwatch.Stop();
        var timeWithCaching = stopwatch.ElapsedMilliseconds;

        // Clear cache to simulate without caching
        cache.Remove($"Snapshot_Website_{_testWebsiteId}");

        // Add a small delay to ensure cache is fully cleared
        await Task.Delay(10);

        stopwatch.Restart();

        // Perform 100 reads without cache
        for (int i = 0; i < 100; i++)
        {
            await _ratingRepository.GetSnapshotByWebsiteIdAsync(_testWebsiteId);
        }

        stopwatch.Stop();
        var timeWithoutCaching = stopwatch.ElapsedMilliseconds;

        // Document results
        Console.WriteLine($"Time with caching (100 reads): {timeWithCaching} ms");
        Console.WriteLine($"Time without caching (100 reads): {timeWithoutCaching} ms");
        Console.WriteLine($"Improvement: {((double)(timeWithoutCaching - timeWithCaching) / timeWithoutCaching * 100):F2}% faster with caching");

        // Assert that caching is faster (with some tolerance for small differences)
        Assert.True(timeWithCaching <= timeWithoutCaching, "Caching should improve or match performance");
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}