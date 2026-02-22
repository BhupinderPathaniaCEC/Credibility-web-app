using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CredibilityIndex.Infrastructure.Persistence
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<CredibilityDbContext>();

            // Check if categories already exist
            if (context.Categories.Any())
                return;

            var categories = new List<Category>
            {
                new Category
                {
                    Name = "News",
                    Slug = "news",
                    Description = "News and journalism credibility",
                    IsActive = true
                },
                new Category
                {
                    Name = "Social Media",
                    Slug = "social-media",
                    Description = "Social media and viral content credibility",
                    IsActive = true
                },
                new Category
                {
                    Name = "Claims",
                    Slug = "claims",
                    Description = "Fact-checking and claims verification",
                    IsActive = true
                },
                new Category
                {
                    Name = "Health & Science",
                    Slug = "health-science",
                    Description = "Health and scientific information credibility",
                    IsActive = true
                },
                new Category
                {
                    Name = "Politics",
                    Slug = "politics",
                    Description = "Political statements and claims credibility",
                    IsActive = true
                },
                new Category
                {
                    Name = "Business",
                    Slug = "business",
                    Description = "Business and financial information credibility",
                    IsActive = true
                },
                new Category
                {
                    Name = "Entertainment",
                    Slug = "entertainment",
                    Description = "Entertainment and celebrity news credibility",
                    IsActive = true
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
