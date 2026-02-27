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
}