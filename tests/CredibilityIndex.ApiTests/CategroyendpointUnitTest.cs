using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Api.Contracts.Category;

namespace CredibilityIndex.ApiTests;

public class CategoriesControllerUpdateTests
{
    private readonly Mock<ICategoryRepository> _mockRepo;
    private readonly CategoriesController _controller;

    public CategoriesControllerUpdateTests()
    {
        _mockRepo = new Mock<ICategoryRepository>();
        _controller = new CategoriesController(_mockRepo.Object);
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
    public async Task ToggleStatus_ReturnsNotFound_WhenRepoThrowsException()
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
}