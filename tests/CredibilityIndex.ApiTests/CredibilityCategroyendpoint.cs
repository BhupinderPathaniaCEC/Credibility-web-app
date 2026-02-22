using CredibilityIndex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CredibilityIndex.Domain.Entities;
public class CategoryApiTests
{
    [Fact]
    public async Task GetActiveCategories_ShouldOnlyReturnActiveItems()
    {
        // 1. ARRANGE
        // Setup a database with 1 Active and 1 Disabled category
        var options = new DbContextOptionsBuilder<CredibilityDbContext>()
            .UseInMemoryDatabase(databaseName: "CategoryTestDb")
            .Options;

        using var context = new CredibilityDbContext(options);
        context.Categories.Add(new Category { Name = "Active Cat",Slug = "active-cat", IsActive = true, Description = "Test" });
        context.Categories.Add(new Category { Name = "Hidden Cat", Slug = "hidden-cat", IsActive = false, Description = "Test" });
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);

        // 2. ACT
        var result = await repository.GetActiveCategoriesAsync();

        // 3. ASSERT
        Assert.Single(result); // Verifies only 1 category returned
        Assert.All(result, c => Assert.True(c.IsActive)); // Verifies it is the active one
        Assert.DoesNotContain(result, c => c.Name == "Hidden Cat"); // Verifies exclusion
    }
}