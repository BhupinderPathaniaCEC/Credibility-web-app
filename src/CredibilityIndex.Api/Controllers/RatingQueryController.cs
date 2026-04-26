using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CredibilityIndex.Api.Contracts.Rating;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Application.Common;

namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Route("api/v1/websites")] // Matches the route in your Acceptance Criteria
    public class RatingQueryController : ControllerBase
    {
        private readonly IRatingQueryRepository _queryRepository;
        private readonly IWebsiteRepository _websiteRepository;

        // Inject our new Read-Only repo alongside the Website repo
        public RatingQueryController( IRatingQueryRepository queryRepository, IWebsiteRepository websiteRepository)
        {
            _queryRepository = queryRepository;
            _websiteRepository = websiteRepository;
        }

        [HttpGet("{domain}/ratings")]
        [AllowAnonymous] // Anyone can view ratings!
        public async Task<IActionResult> GetWebsiteRatings([FromRoute] string domain, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // 1. Normalize and validate the domain
            var normalizedDomain = DomainUtility.NormalizeDomain(domain);
            if (string.IsNullOrEmpty(normalizedDomain))
                return BadRequest(new { message = "Invalid domain format." });

            // 2. Check if website exists
            var website = await _websiteRepository.GetByNormalizedDomainAsync(normalizedDomain);
            if (website == null)
                return NotFound(new { message = $"No website found for domain: {domain}" });

            // 3. Fetch paginated data from our new dedicated repository
            var (ratings, totalCount) = await _queryRepository.GetPaginatedRatingsAsync(website.Id, page, pageSize);

            // 4. Map to the response DTO
            var response = new PaginatedRatingsResponse
            {
                Domain = website.Domain,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = ratings.Select(r => new RatingItemResponse
                {
                    Id = r.Id,
                    Accuracy = r.Accuracy,
                    BiasNeutrality = r.BiasNeutrality,
                    Transparency = r.Transparency,
                    SafetyTrust = r.SafetyTrust,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    DisplayName = "Anonymous Reviewer" 
                }).ToList()
            };

            return Ok(response);
        }
    }
}