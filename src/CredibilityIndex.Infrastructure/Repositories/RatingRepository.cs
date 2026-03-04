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

        public async Task<RatingEntity?> GetByUserAndWebsiteAsync(Guid userId, Guid websiteId)
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

            // 3. Save to SQL database
            await _context.SaveChangesAsync();
        }

        public async Task<double> GetAverageCredibilityAsync(Guid websiteId)
        {
            // Minimal: Averages the 4 dimensions from your diagram
            return await _context.Ratings
                .Where(r => r.WebsiteId == websiteId)
                .AverageAsync(r => (double)(r.Accuracy + r.BiasNeutrality + r.Transparency + r.SafetyTrust) / 4);
        }
    }
}