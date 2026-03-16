using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Application.Common;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CredibilityIndex.Infrastructure.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly CredibilityDbContext _context;

        public RatingRepository(CredibilityDbContext context) => _context = context;

        public async Task<RatingEntity?> GetByUserAndWebsiteAsync(Guid userId, int websiteId)
            => await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.WebsiteId == websiteId);

        public async Task<CredibilitySnapshot> UpsertRatingAsync(RatingEntity incomingRating)
        {
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

        public async Task<double> GetAverageCredibilityAsync(int websiteId)
        {
            // Minimal: Averages the 4 dimensions from your diagram
            return await _context.Ratings
                .Where(r => r.WebsiteId == websiteId)
                .AverageAsync(r => (double)(r.Accuracy + r.BiasNeutrality + r.Transparency + r.SafetyTrust) / 4);
        }
        // Add this single method inside your RatingRepository class:
        public async Task<CredibilitySnapshot?> GetSnapshotByWebsiteIdAsync(int websiteId)
        {
            return await _context.CredibilitySnapshots
                .FirstOrDefaultAsync(s => s.WebsiteId == websiteId);
        }

        // Add this method inside your RatingRepository class:
        public async Task<CredibilitySnapshot?> GetSnapshotByDomainAsync(string normalizedDomain)
        {
            // 1. Find the website by its normalized string
            var website = await _context.Websites
                .FirstOrDefaultAsync(w => w.Domain == normalizedDomain);

            if (website == null)
                return null; // Website doesn't exist in our system yet

            // 2. Return the matching snapshot using the ID we just found
            return await _context.CredibilitySnapshots
                .FirstOrDefaultAsync(s => s.WebsiteId == website.Id);
        }
    }
}