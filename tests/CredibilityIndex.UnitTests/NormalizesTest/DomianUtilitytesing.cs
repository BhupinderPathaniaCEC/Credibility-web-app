using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Controllers;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Application.Common;

namespace CredibilityIndex.ApiTests;

public class DomainUtilityTests
{
    private readonly Mock<ICategoryRepository> _mockRepo;
    private readonly CategoriesController _controller;

    public DomainUtilityTests()
    {
        _mockRepo = new Mock<ICategoryRepository>();
        _controller = new CategoriesController(_mockRepo.Object);
    }

    [Fact]
    public void NormalizeDomain_ShouldReturnCanonicalForm()
    {
        // Arrange
        var input = "  https://WWW.BBC.com/news/uk-123  ";
        var expected = "bbc.com";

        // Act
        var result = DomainUtility.NormalizeDomain(input);

        // Assert
        Assert.Equal(expected, result);
    }
}