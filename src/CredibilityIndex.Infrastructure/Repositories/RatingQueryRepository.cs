using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CredibilityIndex.Infrastructure.Repositories
{
    public class RatingQueryRepository : IRatingQueryRepository
    {
        private readonly CredibilityDbContext _context;

        public RatingQueryRepository(CredibilityDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<RatingEntity> Ratings, int TotalCount)> GetPaginatedRatingsAsync(int websiteId, int page, int pageSize)
        {
            // 1. Filter by website
            var query = _context.Ratings.Where(r => r.WebsiteId == websiteId);

            // 2. Get the total count for the Angular paginator
            var totalCount = await query.CountAsync();

            // 3. Apply sorting and pagination
            var ratings = await query
                .OrderByDescending(r => r.CreatedAt) // Newest first
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (ratings, totalCount);
        }
    }
}