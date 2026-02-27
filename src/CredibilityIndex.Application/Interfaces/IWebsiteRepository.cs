using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Application.Interfaces;

public interface IWebsiteRepository
{
    Task<IEnumerable<Website>> SearchWebsitesAsync(string query);
    Task AddAsync(Website website);
    Task<Website?> GetByIdAsync(int id);
}