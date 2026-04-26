using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Application.Interfaces
{
    public interface IRatingQueryRepository
    {
        Task<(IEnumerable<RatingEntity> Ratings, int TotalCount)> GetPaginatedRatingsAsync(int websiteId, int page, int pageSize);
    }
}