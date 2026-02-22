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
        var categoryToUpdate = await _context.Categories.FindAsync(category.Id);
        if (categoryToUpdate != null)        {
            categoryToUpdate.Name = category.Name;
            categoryToUpdate.Description = category.Description;
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