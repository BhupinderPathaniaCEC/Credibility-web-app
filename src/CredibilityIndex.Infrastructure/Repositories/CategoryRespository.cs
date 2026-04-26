using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using Microsoft.EntityFrameworkCore; // This is required for AnyAsync and ToListAsync   

namespace CredibilityIndex.Infrastructure.Persistence;

public class CategoryRepository : ICategoryRepository
{
    // Static list so data persists as long as the app is running

    private readonly CredibilityDbContext _context;

    public CategoryRepository(CredibilityDbContext context)
    {
        _context = context;
    }

    // 1. Implement GetByNameAsync
        public async Task<Category?> GetByNameAsync(string name)
        {
            // Use EF Core to find the first category that matches the name
            // (Using ToLower() or checking the Slug is even safer if you want!)
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        // 2. Implement SaveChangesAsync
        public async Task SaveChangesAsync()
        {
            // Simply pass the save command down to the database context
            await _context.SaveChangesAsync();
        }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        return await _context.Categories.Where(c => c.IsActive).ToListAsync();
    }



   public async Task UpdateAsync(Category category)
{
    // Find the existing entity in the database
    var categoryToUpdate = await _context.Categories.FindAsync(category.Id);

    if (categoryToUpdate != null)
    {
        // Update the fields
        categoryToUpdate.Name = category.Name;
        categoryToUpdate.Description = category.Description;
        
        // Ensure the slug is also updated to keep URLs in sync
        categoryToUpdate.Slug = category.Slug; 
        
        // EF Core tracks these changes and generates the UPDATE SQL
        await _context.SaveChangesAsync();
    }
}

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ToggleStatusAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)        {
            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();
            return category.IsActive;
        }

        throw new Exception("Category not found");
    }
}