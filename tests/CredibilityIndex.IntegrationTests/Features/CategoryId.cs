using System.Net;
using System.Net.Http.Json;
using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.IntegrationTests;

public class CategoriesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CategoriesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActiveCategories_ReturnsSuccessStatusCode()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/v1/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveCategories_ReturnsListOfCategories()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/v1/categories");
        var categories = await response.Content.ReadFromJsonAsync<List<Category>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(categories);
        Assert.IsType<List<Category>>(categories);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var response = await _client.GetAsync($"/api/v1/categories/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
