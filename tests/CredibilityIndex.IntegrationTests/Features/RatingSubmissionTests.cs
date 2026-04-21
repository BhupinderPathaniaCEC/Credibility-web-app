using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using CredibilityIndex.Api.Contracts.Auth;
using CredibilityIndex.Api.Contracts.Rating;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using System.Collections.Generic;

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
    public async Task SubmitRating_WithValidToken_ShouldCreateRatingAndReturnCredibility()
    {
        // Arrange - Register a user first
        var registerRequest = new RegisterRequest(
            Email: "ratinguser@example.com",
            Password: "TestPass123!",
            DisplayName: "Rating User"
        );

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Get access token
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "ratinguser@example.com",
            ["password"] = "TestPass123!",
            ["client_id"] = "mvp-client",
            ["client_secret"] = "super-secret",
            ["scope"] = "openid profile email"
        };

        var tokenResponse = await _client.PostAsync("/connect/token", new FormUrlEncodedContent(tokenRequest));
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

        var tokenData = await tokenResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(tokenData);
        var accessToken = tokenData["access_token"].ToString();
        Assert.NotNull(accessToken);

        // Create authenticated client
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act - Submit rating
        var ratingRequest = new CreateRatingRequest
        {
            RawUrl = "https://testcredibility.com",
            DisplayName = "Test Credibility Site",
            Accuracy = 4,
            Transparency = 3,
            BiasNeutrality = 4,
            SafetyTrust = 5,
            Comment = "Good website for testing"
        };

        var submitResponse = await authenticatedClient.PutAsJsonAsync("/api/v1/websites/testcredibility.com/ratings", ratingRequest);

        // Assert - Should return OK with credibility data
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var credibilityResponse = await submitResponse.Content.ReadFromJsonAsync<UpdatedSnapshotResponse>();
        Assert.NotNull(credibilityResponse);
        Assert.True(credibilityResponse.WebsiteId > 0);
        Assert.Equal(4.0, credibilityResponse.AvgAccuracy);
        Assert.Equal(3.0, credibilityResponse.AvgTransparency);
        Assert.Equal(4.0, credibilityResponse.AvgBiasNeutrality);
        Assert.Equal(5.0, credibilityResponse.AvgSafetyTrust);
        Assert.Equal(1, credibilityResponse.RatingCount);
        Assert.True(credibilityResponse.Score0to100 > 0);
        Assert.True(credibilityResponse.ConfidenceScore > 0);
    }

    [Fact]
    public async Task GetCredibilityByDomain_ShouldReturnCredibilityData_AfterRatingSubmission()
    {
        // Arrange - Register user and submit rating first
        var registerRequest = new RegisterRequest(
            Email: "credibilityreader@example.com",
            Password: "TestPass123!",
            DisplayName: "Credibility Reader"
        );

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Get access token
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "credibilityreader@example.com",
            ["password"] = "TestPass123!",
            ["client_id"] = "mvp-client",
            ["client_secret"] = "super-secret",
            ["scope"] = "openid profile email"
        };

        var tokenResponse = await _client.PostAsync("/connect/token", new FormUrlEncodedContent(tokenRequest));
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

        var tokenData = await tokenResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(tokenData);
        var accessToken = tokenData["access_token"].ToString();
        Assert.NotNull(accessToken);

        // Create authenticated client
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Submit rating
        var ratingRequest = new CreateRatingRequest
        {
            RawUrl = "https://readcredibility.com",
            DisplayName = "Read Credibility Site",
            Accuracy = 5,
            Transparency = 4,
            BiasNeutrality = 3,
            SafetyTrust = 4,
            Comment = "Excellent site"
        };

        var submitResponse = await authenticatedClient.PutAsJsonAsync("/api/v1/websites/readcredibility.com/ratings", ratingRequest);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        // Act - Read credibility (no auth required)
        var credibilityResponse = await _client.GetAsync("/api/v1/websites/readcredibility.com/credibility");

        // Assert
        Assert.Equal(HttpStatusCode.OK, credibilityResponse.StatusCode);

        var credibilityData = await credibilityResponse.Content.ReadFromJsonAsync<UpdatedSnapshotResponse>();
        Assert.NotNull(credibilityData);
        Assert.True(credibilityData.WebsiteId > 0);
        Assert.Equal(5.0, credibilityData.AvgAccuracy);
        Assert.Equal(4.0, credibilityData.AvgTransparency);
        Assert.Equal(3.0, credibilityData.AvgBiasNeutrality);
        Assert.Equal(4.0, credibilityData.AvgSafetyTrust);
        Assert.Equal(1, credibilityData.RatingCount);
        Assert.True(credibilityData.Score0to100 > 0);
        Assert.True(credibilityData.ConfidenceScore > 0);
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