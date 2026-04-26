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

            var snapshot = await _context.CredibilitySnapshots
                .FirstOrDefaultAsync(s => s.WebsiteId == incomingRating.WebsiteId);

            if (snapshot == null)
            {
                snapshot = new CredibilitySnapshot { WebsiteId = incomingRating.WebsiteId, RatingCount = 0 };
                await _context.CredibilitySnapshots.AddAsync(snapshot);
            }

            if (existingRating != null)
            {
                // 1. Let the Application Layer do the complex math
                SnapshotCalculator.ApplyUpdatedRating(snapshot, existingRating, incomingRating);

                // 2. Update the DB record
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
                // 1. Let the Application Layer do the complex math
                SnapshotCalculator.ApplyNewRating(snapshot, incomingRating);

                // 2. Add the new DB record
                await _context.Ratings.AddAsync(incomingRating);
            }

            await _context.SaveChangesAsync();

            // 3. Return the fresh data so the Controller can send it to Angular
            return snapshot;
           
        }

        public async Task<double> GetAverageCredibilityAsync(int websiteId)
        {
            // Minimal: Averages the 4 dimensions from your diagram
            return await _context.Ratings
                .Where(r => r.WebsiteId == websiteId)
                .AverageAsync(r => (double)(r.Accuracy + r.BiasNeutrality + r.Transparency + r.SafetyTrust) / 4);
        }
    }
}