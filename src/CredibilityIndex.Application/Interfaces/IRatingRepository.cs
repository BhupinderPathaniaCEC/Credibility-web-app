using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Application.Interfaces
{
    public interface IRatingRepository
    {
        Task<RatingEntity?> GetByUserAndWebsiteAsync(Guid userId, int websiteId);
        Task UpsertRatingAsync(RatingEntity rating);
        Task<double> GetAverageCredibilityAsync(int websiteId);
    }
}