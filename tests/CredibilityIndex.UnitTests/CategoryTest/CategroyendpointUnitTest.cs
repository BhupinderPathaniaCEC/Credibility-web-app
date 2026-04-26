using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Api.Contracts.Category;

namespace CredibilityIndex.ApiTests;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryRepository> _mockRepo;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mockRepo = new Mock<ICategoryRepository>();
        _controller = new CategoriesController(_mockRepo.Object);
    }

    // --- GET ACTIVE CATEGORIES TESTS ---
    [Fact]
    public async Task GetActiveCategories_ReturnsOk_WithCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "News", Slug = "news", IsActive = true },
            new() { Id = 2, Name = "Tech", Slug = "tech", IsActive = true }
        };

        _mockRepo.Setup(r => r.GetActiveCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.GetActiveCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<Category>>();
        _mockRepo.Verify(r => r.GetActiveCategoriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetActiveCategories_ReturnsOk_WithEmptyList()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveCategoriesAsync()).ReturnsAsync(new List<Category>());

        // Act
        var result = await _controller.GetActiveCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<Category>>();
    }

    // --- GET CATEGORY BY ID TESTS ---
    [Fact]
    public async Task GetCategoryById_ReturnsOk_WhenCategoryExists()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "News", Slug = "news", IsActive = true };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        // Act
        var result = await _controller.GetCategoryById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(category);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category)null);

        // Act
        var result = await _controller.GetCategoryById(99);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotFound_WhenCategoryIsInactive()
    {
        // Arrange
        var inactiveCategory = new Category { Id = 1, Name = "Old", Slug = "old", IsActive = false };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inactiveCategory);

        // Act
        var result = await _controller.GetCategoryById(1);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // --- CREATE CATEGORY TESTS ---
    [Fact]
    public async Task CreateCategory_ReturnsCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Technology",
            Slug = "technology",
            Description = "Tech-related sites",
            IsActive = true
        };

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetCategoryById));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "", Slug = "" };
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateCategory_GeneratesSlugFromName_WhenSlugNotProvided()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Technology News",
            Description = "Tech news",
            IsActive = true
        };

        Category capturedCategory = null;
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Callback<Category>(c => capturedCategory = c)
            .Returns(Task.CompletedTask);

        // Act
        await _controller.CreateCategory(request);

        // Assert
        capturedCategory.Should().NotBeNull();
        capturedCategory.Slug.Should().Be("technology-news");
    }

    // --- UPDATE TESTS ---
    [Fact]
    public async Task UpdateCategory_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var existing = new Category { Id = 1, Name = "Old Name", Slug = "test-slug" };
        var request = new UpdateCategoryRequest { Name = "New Name" };
        
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        // Act
        var result = await _controller.UpdateCategory(1, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    // --- DELETE TESTS ---
    [Fact]
    public async Task DeleteCategory_ReturnsBadRequest_WhenCategoryIsStillActive()
    {
        // Arrange
        var activeCategory = new Category { Id = 1, IsActive = true, Name = "Test Category", Slug = "test-slug" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeCategory);

        // Act
        var result = await _controller.DeleteCategory(1);

        // Assert
        // Your logic says: if (category.IsActive) return BadRequest
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteCategory_ReturnsOk_WhenCategoryIsInactive()
    {
        // Arrange
        var inactiveCategory = new Category { Id = 1, IsActive = false, Slug = "test-slug", Name= "Test Category" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inactiveCategory);

        // Act
        var result = await _controller.DeleteCategory(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    // --- TOGGLE TESTS ---
    [Fact]
    public async Task ToggleCategoryStatus_ReturnsOk_WhenTogglesToActive()
    {
        // Arrange
        _mockRepo.Setup(r => r.ToggleStatusAsync(1))
                 .ReturnsAsync(true);

        // Act
        var result = await _controller.ToggleCategoryStatus(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.ToString().Should().Contain("active");
        _mockRepo.Verify(r => r.ToggleStatusAsync(1), Times.Once);
    }

    [Fact]
    public async Task ToggleCategoryStatus_ReturnsOk_WhenTogglesToInactive()
    {
        // Arrange
        _mockRepo.Setup(r => r.ToggleStatusAsync(1))
                 .ReturnsAsync(false);

        // Act
        var result = await _controller.ToggleCategoryStatus(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.ToString().Should().Contain("inactive");
    }

    [Fact]
    public async Task ToggleCategoryStatus_ReturnsNotFound_WhenRepoThrowsException()
    {
        // Arrange
        // Your controller catches exceptions and checks for "not found" message
        _mockRepo.Setup(r => r.ToggleStatusAsync(99))
                 .ThrowsAsync(new Exception("Category not found"));

        // Act
        var result = await _controller.ToggleCategoryStatus(99);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ToggleCategoryStatus_ReturnsServerError_WhenOtherExceptionThrown()
    {
        // Arrange
        _mockRepo.Setup(r => r.ToggleStatusAsync(1))
                 .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _controller.ToggleCategoryStatus(1);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }
}