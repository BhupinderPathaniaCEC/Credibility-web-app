using CredibilityIndex.Domain.Entities;
using System;


namespace CredibilityIndex.Application.Interfaces
{
    public interface IRatingRepository
    {
        // 1. Our newly updated Upsert method that returns the calculated math!
        Task<CredibilitySnapshot> UpsertRatingAsync(RatingEntity incomingRating);

        // 2. Your existing methods (updated to use Guid to match your Entities/DB)
        Task<RatingEntity?> GetByUserAndWebsiteAsync(Guid userId, int websiteId);
        Task<double> GetAverageCredibilityAsync(int websiteId);
        Task<CredibilitySnapshot?> GetSnapshotByWebsiteIdAsync(int websiteId);
        Task<CredibilitySnapshot?> GetSnapshotByDomainAsync(string normalizedDomain);
    }
}