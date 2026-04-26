using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Application.Common;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CredibilityIndex.Infrastructure.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly CredibilityDbContext _context;
        private readonly IMemoryCache _cache;

        public RatingRepository(CredibilityDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<RatingEntity?> GetByUserAndWebsiteAsync(Guid userId, int websiteId)
            => await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.WebsiteId == websiteId);

        public async Task<CredibilitySnapshot> UpsertRatingAsync(RatingEntity incomingRating)
        {
            // This method handles both creating new ratings and updating existing ones.
            // After any rating change, the credibility snapshot is recalculated and caches are invalidated
            // to ensure no stale data is served to clients.
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == incomingRating.UserId && r.WebsiteId == incomingRating.WebsiteId);

            // Ensure snapshot exists and is up-to-date
            var snapshot = await EnsureSnapshotExistsAsync(incomingRating.WebsiteId);

            if (existingRating != null)
            {
                // Update existing rating
                SnapshotCalculator.ApplyUpdatedRating(snapshot, existingRating, incomingRating);
                existingRating.Accuracy = incomingRating.Accuracy;
                existingRating.BiasNeutrality = incomingRating.BiasNeutrality;
                existingRating.Transparency = incomingRating.Transparency;
                existingRating.SafetyTrust = incomingRating.SafetyTrust;
                existingRating.Comment = incomingRating.Comment;
                existingRating.UpdatedAt = DateTime.UtcNow;
                _context.Ratings.Update(existingRating);
            }
            else
            {
                // Add new rating
                SnapshotCalculator.ApplyNewRating(snapshot, incomingRating);
                await _context.Ratings.AddAsync(incomingRating);
            }

            // Persist all changes immediately
            await _context.SaveChangesAsync();

            // Invalidate all caches for this website to prevent stale data
            await InvalidateSnapshotCachesAsync(incomingRating.WebsiteId);

            return snapshot;
        }

        private async Task<CredibilitySnapshot> EnsureSnapshotExistsAsync(int websiteId)
        {
            var snapshot = await _context.CredibilitySnapshots
                .FirstOrDefaultAsync(s => s.WebsiteId == websiteId);

            if (snapshot == null)
            {
                snapshot = new CredibilitySnapshot { WebsiteId = websiteId, RatingCount = 0 };
                _context.CredibilitySnapshots.Add(snapshot);
            }

            return snapshot;
        }

        private async Task InvalidateSnapshotCachesAsync(int websiteId)
        {
            // Invalidate both website-based and domain-based caches to ensure
            // no stale credibility data is served after rating changes
            _cache.Remove($"Snapshot_Website_{websiteId}");

            // Also invalidate domain cache by fetching the current domain
            var website = await _context.Websites.FindAsync(websiteId);
            if (website != null)
            {
                _cache.Remove($"Snapshot_Domain_{website.Domain}");
            }
        }

        public async Task<double> GetAverageCredibilityAsync(int websiteId)
        {
            // Minimal: Averages the 4 dimensions from your diagram
            return await _context.Ratings
                .Where(r => r.WebsiteId == websiteId)
                .AverageAsync(r => (double)(r.Accuracy + r.BiasNeutrality + r.Transparency + r.SafetyTrust) / 4);
        }
        public async Task<CredibilitySnapshot?> GetSnapshotByWebsiteIdAsync(int websiteId)
        {
            var cacheKey = $"Snapshot_Website_{websiteId}";
            if (_cache.TryGetValue(cacheKey, out CredibilitySnapshot? snapshot))
            {
                return snapshot;
            }

            snapshot = await _context.CredibilitySnapshots
                .FirstOrDefaultAsync(s => s.WebsiteId == websiteId);

            if (snapshot != null)
            {
                _cache.Set(cacheKey, snapshot, TimeSpan.FromMinutes(5));
            }

            return snapshot;
        }

        // Add this method inside your RatingRepository class:
        public async Task<CredibilitySnapshot?> GetSnapshotByDomainAsync(string normalizedDomain)
        {
            var cacheKey = $"Snapshot_Domain_{normalizedDomain}";
            if (_cache.TryGetValue(cacheKey, out CredibilitySnapshot? snapshot))
            {
                return snapshot;
            }

            // 1. Find the website by its normalized string
            var website = await _context.Websites
                .FirstOrDefaultAsync(w => w.Domain == normalizedDomain);

            if (website == null)
                return null; // Website doesn't exist in our system yet

            // 2. Return the matching snapshot using the ID we just found
            snapshot = await _context.CredibilitySnapshots
                .FirstOrDefaultAsync(s => s.WebsiteId == website.Id);

            if (snapshot != null)
            {
                _cache.Set(cacheKey, snapshot, TimeSpan.FromMinutes(5));
            }

            return snapshot;
        }

        public async Task<RatingEntity?> GetUserRatingForDomainAsync(string normalizedDomain, Guid userId)
        {
            // We use Entity Framework to look inside the Ratings table, 
            // join it to the Websites table, and find the exact match.
            return await _context.Ratings
                .Include(r => r.Website) // Make sure we can access the Website properties
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.Website.Domain == normalizedDomain);
        }
    }
}