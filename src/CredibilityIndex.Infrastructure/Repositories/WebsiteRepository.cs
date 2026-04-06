using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Application.Common; // Using your DomainUtility
using CredibilityIndex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CredibilityIndex.Infrastructure.Persistence;

public class WebsiteRepository : IWebsiteRepository
{
    private readonly CredibilityDbContext _context;

    public WebsiteRepository(CredibilityDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Website>> SearchWebsitesAsync(string query)
    {
        // Acceptance Criteria: Handle empty/no-results clearly
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Website>();

        // Acceptance Criteria: Normalize Query input
        var normalizedQuery = DomainUtility.NormalizeDomain(query);

        // Search in both Name and the normalized Domain
        return await _context.Websites
            .Include(w => w.Category) // Ensure metadata is included
            .Include(w => w.CredibilitySnapshot) // Include snapshot for search summaries
            .Where(w => w.Domain.Contains(normalizedQuery) ||
                        w.Name.ToLower().Contains(query.ToLower()))
            .ToListAsync();
    }

    public async Task AddAsync(Website website)
    {
        // Normalize domain before saving to ensure data integrity
        website.Domain = DomainUtility.NormalizeDomain(website.Domain);
        await _context.Websites.AddAsync(website);
        await _context.SaveChangesAsync();
    }

    public async Task<Website?> GetByIdAsync(int id)
    {
        return await _context.Websites
            .Include(w => w.Category)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Website?> GetWebsiteWithSnapshotAsync(int id)
    {
        return await _context.Websites
            .Include(w => w.Category)
            .Include(w => w.CredibilitySnapshot) // Nest the snapshot here
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Website?> GetWebsiteWithSnapshotByDomainAsync(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) return null;

        // Normalize the incoming route parameter
        var normalizedDomain = DomainUtility.NormalizeDomain(domain);

        return await _context.Websites
            .Include(w => w.Category)
            .Include(w => w.CredibilitySnapshot) // Nest the breakdown
            .FirstOrDefaultAsync(w => w.Domain == normalizedDomain);
    }
    public async Task<Website?> GetByNormalizedDomainAsync(string normalizedDomain)
    {
        // No .Include() needed here, we just need to know if it exists to prevent duplicates
        return await _context.Websites
            .FirstOrDefaultAsync(w => w.Domain == normalizedDomain);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Website>> GetAllWithCategoriesAsync()
    {
        // The .Include() tells EF Core to grab the linked Category data too!
        return await _context.Websites
            .Include(w => w.Category)
            .ToListAsync();
    }


}