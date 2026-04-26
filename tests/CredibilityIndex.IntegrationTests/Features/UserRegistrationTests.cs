using System.Net;
using System.Net.Http.Json;
using CredibilityIndex.Api.Contracts.Auth;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Infrastructure.Auth;
using CredibilityIndex.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CredibilityIndex.IntegrationTests.Features;

public class UserRegistrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public UserRegistrationTests(CustomWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldCreateUserInDatabase()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "testuser@example.com",
            Password: "TestPass123!",
            DisplayName: "Test User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Debug: Log the response if not successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Response status: {response.StatusCode}");
            _testOutputHelper.WriteLine($"Error content: {errorContent}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(registerResponse);
        Assert.Equal("testuser@example.com", registerResponse.Email);
        Assert.Equal("Test User", registerResponse.DisplayName);
        Assert.NotNull(registerResponse.Id);
        Assert.NotEmpty(registerResponse.Id);

        // Verify user was created in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CredibilityDbContext>();
        var createdUser = await dbContext.Users.FindAsync(registerResponse.Id);
        Assert.NotNull(createdUser);
        Assert.Equal("testuser@example.com", createdUser.Email);
        Assert.Equal("Test User", createdUser.DisplayName);
        Assert.Equal("testuser@example.com", createdUser.UserName);
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange - First create a user
        var firstRequest = new RegisterRequest(
            Email: "duplicate@example.com",
            Password: "TestPass123!",
            DisplayName: "First User"
        );

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", firstRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act - Try to register with same email
        var duplicateRequest = new RegisterRequest(
            Email: "duplicate@example.com",
            Password: "DiffPass123!",
            DisplayName: "Second User"
        );

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", duplicateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new RegisterRequest(
            Email: "invalid-email",
            Password: "TestPass123!",
            DisplayName: "Test User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterUser_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var weakPasswordRequest = new RegisterRequest(
            Email: "weakpass@example.com",
            Password: "123", // Too short
            DisplayName: "Test User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", weakPasswordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterUser_WithEmptyDisplayName_ShouldReturnBadRequest()
    {
        // Arrange
        var emptyDisplayNameRequest = new RegisterRequest(
            Email: "emptydisplay@example.com",
            Password: "TestPass123!",
            DisplayName: "" // Empty
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", emptyDisplayNameRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}