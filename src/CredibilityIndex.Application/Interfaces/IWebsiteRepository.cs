using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Application.Interfaces;

public interface IWebsiteRepository
{
    Task<Website?> GetWebsiteWithSnapshotByDomainAsync(string domain);
    Task<IEnumerable<Website>> SearchWebsitesAsync(string query);
    Task<Website?> GetByNormalizedDomainAsync(string normalizedDomain);
    Task AddAsync(Website website);
    // Task<Website> GetOrCreateWebsiteForRatingAsync(string rawUrl);
    Task<Website?> GetByIdAsync(int id);
    Task SaveChangesAsync();
    Task<IEnumerable<Website>> GetAllWithCategoriesAsync();
}