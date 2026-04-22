using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using CredibilityIndex.Api.Contracts.Auth;
using CredibilityIndex.Api.Contracts.Rating;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using System.Collections.Generic;
using CredibilityIndex.IntegrationTests.Infrastructure;

namespace CredibilityIndex.IntegrationTests.Features;

public class RatingSubmissionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public RatingSubmissionTests(CustomWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SubmitRating_RequiresAuthorization()
    {
        // Arrange
        var ratingRequest = new CreateRatingRequest
        {
            RawUrl = "https://example.com",
            DisplayName = "Example Site",
            Accuracy = 4,
            Transparency = 3,
            BiasNeutrality = 4,
            SafetyTrust = 5,
            Comment = "Good website"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/websites/example.com/ratings", ratingRequest);

        // Assert - Should return 401 Unauthorized since no authentication
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWebsiteRatings_ReturnsNotFound_WhenWebsiteDoesNotExist()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/v1/websites/nonexistent.com/ratings");

        // Assert - Should return NotFound for non-existent website
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubmitRating_WithFakeToken_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // This line tells the API: "I am logged in via the TestScheme"
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.TestScheme);

        var ratingRequest = new
        {
            RawUrl = "https://testcredibility.com",
            DisplayName = "Test Site",
            Accuracy = 5,
            Transparency = 5,
            BiasNeutrality = 5,
            SafetyTrust = 5,
            Comment = "Testing with fake token"
        };

        // Act
        var response = await client.PutAsJsonAsync("api/v1/websites/testcredibility.com/ratings", ratingRequest);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"VALIDATION FAILED: {errorJson}");
        }
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
public async Task GetCredibilityByDomain_ShouldReturnCredibilityData_AfterRatingSubmission()
{
    // 1. ARRANGE - Setup the authenticated client
    var client = _factory.CreateClient();
    
    // Use the Fake Token Scheme
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue(TestAuthHandler.TestScheme);

    var domain = "example.com";
    var ratingRequest = new 
    { 
        RawUrl = $"https://{domain}", 
        DisplayName = "Example Site",
        Accuracy = 5, 
        Transparency = 4,
        BiasNeutrality = 5,
        SafetyTrust = 4,
        Comment = "Initial rating for integration test" 
    };

    // 2. ACT - Phase 1: Submit the rating (Requires Auth)
    var submitResponse = await client.PutAsJsonAsync($"api/v1/websites/{domain}/ratings", ratingRequest);
    Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

    // 3. ACT - Phase 2: Get Credibility (Public endpoint, can clear auth if you want)
    client.DefaultRequestHeaders.Authorization = null; 
    var credibilityResponse = await client.GetAsync($"api/v1/websites/{domain}/credibility");

    // 4. ASSERT
    Assert.Equal(HttpStatusCode.OK, credibilityResponse.StatusCode);

    var result = await credibilityResponse.Content.ReadFromJsonAsync<UpdatedSnapshotResponse>();
    Assert.NotNull(result);
    Assert.Equal(1, result.RatingCount);
    Assert.Equal(5, result.AvgAccuracy);
    Assert.Equal(4, result.AvgTransparency);
}

    [Fact]
    public async Task GetCredibilityByDomain_ReturnsNotFound_WhenNoRatingsExist()
    {
        // Act - Try to get credibility for a domain with no ratings
        var response = await _client.GetAsync("/api/v1/websites/noratings.com/credibility");

        // Debug: Check response content
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var content = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Internal Server Error: {content}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}