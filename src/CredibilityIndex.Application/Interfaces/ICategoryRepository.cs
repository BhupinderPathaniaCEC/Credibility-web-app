using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Application.Interfaces;

public interface ICategoryRepository
{
    Task AddAsync(Category category);

    Task<Category?> GetByIdAsync(int id);

    Task<IEnumerable<Category>> GetActiveCategoriesAsync();

    Task<IEnumerable<Category>> GetAllAsync();

    Task UpdateAsync(Category category);

    Task DeleteAsync(int id);

    Task<bool> ToggleStatusAsync(int id);
}