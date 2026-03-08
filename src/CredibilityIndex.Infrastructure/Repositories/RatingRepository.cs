using CredibilityIndex.Application.Interfaces;
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

        public async Task UpsertRatingAsync(RatingEntity incomingRating)
        {
            // 1. Find the existing rating for this User + Website
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == incomingRating.UserId && r.WebsiteId == incomingRating.WebsiteId);

            if (existingRating != null)
            {
                // 2a. UPDATE: Map the new scores onto the existing database record
                existingRating.Accuracy = incomingRating.Accuracy;
                existingRating.BiasNeutrality = incomingRating.BiasNeutrality;
                existingRating.Transparency = incomingRating.Transparency;
                existingRating.SafetyTrust = incomingRating.SafetyTrust;
                existingRating.Comment = incomingRating.Comment;
                existingRating.UpdatedAt = DateTime.UtcNow; // Good practice since your entity supports it!

                _context.Ratings.Update(existingRating);
            }
            else
            {
                // 2b. INSERT: This is a brand new rating
                await _context.Ratings.AddAsync(incomingRating);
            }

            // 3. After saving the rating, we need to recalculate the CredibilitySnapshot for the associated website
            var rating = await _context.Ratings
                .Include(r => r.Website)
                    .ThenInclude(w => w.CredibilitySnapshot)
                .Include(r => r.Website)
                    .ThenInclude(w => w.Ratings)
                .FirstOrDefaultAsync(r => r.Id == incomingRating.Id);

            /* 
            Rating (PK:289) -> Website (PK:5)   -> CredibilitySnapshot (PK:2)
                                                -> Ratings (PK:289)
                                                -> Ratings (PK:101)
                                                -> Ratings (PK:302)
                                                -> Ratings (PK:97)
                                                -> Ratings (PK:45)
                                                -> Ratings (PK:12)
            */

            // Calculate and update the CredibilitySnapshot for the website
            double avgAccuracy = rating.Website.Ratings.Average(r => r.Accuracy);
            double avgBiasNeutrality = rating.Website.Ratings.Average(r => r.BiasNeutrality);
            double avgTransparency = rating.Website.Ratings.Average(r => r.Transparency);
            double avgSafetyTrust = rating.Website.Ratings.Average(r => r.SafetyTrust);
            int ratingCount = rating.Website.Ratings.Count;

            // Calculate overall score (as byte, 0-100 scale)
            double avgScore = (avgAccuracy + avgBiasNeutrality + avgTransparency + avgSafetyTrust) / 4.0;
            byte score = (byte)Math.Round(avgScore / 5.0 * 100); // scale 1-5 to 0-100

            var snapshot = rating.Website.CredibilitySnapshot;
            if (snapshot == null)
            {
                // Create new snapshot
                snapshot = new CredibilityIndex.Domain.Entities.CredibilitySnapshot
                {
                    WebsiteId = rating.WebsiteId,
                    AvgAccuracy = avgAccuracy,
                    AvgBiasNeutrality = avgBiasNeutrality,
                    AvgTransparency = avgTransparency,
                    AvgSafetyTrust = avgSafetyTrust,
                    RatingCount = ratingCount,
                    Score = score,
                    ComputedAt = DateTime.UtcNow
                };
                await _context.CredibilitySnapshots.AddAsync(snapshot);
                rating.Website.CredibilitySnapshot = snapshot;
            }
            else
            {
                // Update existing snapshot
                snapshot.AvgAccuracy = avgAccuracy;
                snapshot.AvgBiasNeutrality = avgBiasNeutrality;
                snapshot.AvgTransparency = avgTransparency;
                snapshot.AvgSafetyTrust = avgSafetyTrust;
                snapshot.RatingCount = ratingCount;
                snapshot.Score = score;
                snapshot.ComputedAt = DateTime.UtcNow;
                _context.CredibilitySnapshots.Update(snapshot);
            }

            // 4. Save to SQL database
            await _context.SaveChangesAsync();
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