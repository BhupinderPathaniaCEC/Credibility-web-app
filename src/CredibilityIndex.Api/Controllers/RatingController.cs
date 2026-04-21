using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Contracts.Rating;
using CredibilityIndex.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Application.Common;
using Microsoft.Extensions.Logging;


namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Route("api/v1/websites")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IWebsiteRepository _websiteRepository; // NEW: Injecting Website interface
        private readonly ICategoryRepository _categoryRepository; // NEW: Injecting Category interface
        private readonly ILogger<RatingController> _logger;

        public RatingController(IRatingRepository ratingRepository, IWebsiteRepository websiteRepository, ICategoryRepository categoryRepository, ILogger<RatingController> logger)
        {
            _ratingRepository = ratingRepository;
            _websiteRepository = websiteRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;

        }

        // --- THE MATH HELPER ---
        // Fulfills the "Confidence increases as ratingCount grows" criteria
        private int CalculateConfidenceScore(int ratingCount)
        {
            double rawConfidence = ((double)ratingCount / (ratingCount + 10.0)) * 100.0;
            return (int)Math.Clamp(Math.Round(rawConfidence), 0, 100);
        }

        [HttpGet("{domain}/ratings/me")]
        [Authorize]
        public async Task<IActionResult> GetMyUserRating(string domain)
        {
            var decodedDomain = Uri.UnescapeDataString(domain);
            var normalizedDomain = DomainUtility.NormalizeDomain(decodedDomain);
            var userIdString = User.FindFirst(Claims.Subject)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // You will need to make sure your repository has a method to get a single user's rating by domain
            var existingRating = await _ratingRepository.GetUserRatingForDomainAsync(normalizedDomain, userId);

            if (existingRating == null)
            {
                // Returning Ok(null) or NoContent() tells Angular this is a SUCCESS, 
                // the data is just completely empty!
                return NoContent();
            }

            return Ok(existingRating);
        }

        [HttpGet("/api/v1/me/ratings")]
        [Authorize]
        public async Task<IActionResult> GetMyRatingsDashboard()
        {
            var userIdString = User.FindFirst(Claims.Subject)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Invalid or missing user token." });
            }

            // 1. Get the raw entities from the repository
            var rawRatings = await _ratingRepository.GetMyRatingsAsync(userId);

            // 2. Handle the empty state
            if (rawRatings == null || !rawRatings.Any())
            {
                return Ok(new List<MyRatingResponse>());
            }

            // 3. Map the entities to the Contract and calculate the math right here!
            var responseList = rawRatings.Select(r => new MyRatingResponse
            {
                WebsiteId = r.WebsiteId,
                Domain = r.Website.Domain, // This works because of .Include(r => r.Website)
                Accuracy = r.Accuracy,
                BiasNeutrality = r.BiasNeutrality,
                Transparency = r.Transparency,
                SafetyTrust = r.SafetyTrust,
                PersonalAverageScore = (r.Accuracy + r.BiasNeutrality + r.Transparency + r.SafetyTrust) / 4.0
            });

            return Ok(responseList);
        }

        // --- THE GET ENDPOINT ---
        [HttpGet("{websiteId}/credibility")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWebsiteCredibility(int websiteId)
        {
            var snapshotEntity = await _ratingRepository.GetSnapshotByWebsiteIdAsync(websiteId);

            if (snapshotEntity == null)
                return NotFound(new { message = "No ratings found for this website yet." });

            var response = new UpdatedSnapshotResponse
            {
                WebsiteId = snapshotEntity.WebsiteId,
                Score0to100 = snapshotEntity.Score,
                AvgAccuracy = snapshotEntity.AvgAccuracy,
                AvgBiasNeutrality = snapshotEntity.AvgBiasNeutrality,
                AvgTransparency = snapshotEntity.AvgTransparency,
                AvgSafetyTrust = snapshotEntity.AvgSafetyTrust,
                RatingCount = snapshotEntity.RatingCount,
                ComputedAt = snapshotEntity.ComputedAt,
                ConfidenceScore = CalculateConfidenceScore(snapshotEntity.RatingCount) // Apply Math
            };

            return Ok(response);
        }

        // --- THE NEW GET BY DOMAIN ENDPOINT ---
        // Fulfills the "fetch credibility by domain" user story
        [HttpGet("{domain}/credibility")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCredibilityByDomain(string domain)
        {
            var decodedDomain = Uri.UnescapeDataString(domain);
            var normalizedDomain = DomainUtility.NormalizeDomain(decodedDomain);
            if (string.IsNullOrEmpty(normalizedDomain))
                return BadRequest(new { message = "Invalid domain format." });

            var snapshotEntity = await _ratingRepository.GetSnapshotByDomainAsync(normalizedDomain);

            if (snapshotEntity == null)
                return NotFound(new { message = $"No credibility data found for {normalizedDomain}." });

            var response = new UpdatedSnapshotResponse
            {
                WebsiteId = snapshotEntity.WebsiteId,
                Score0to100 = snapshotEntity.Score,
                AvgAccuracy = snapshotEntity.AvgAccuracy,
                AvgBiasNeutrality = snapshotEntity.AvgBiasNeutrality,
                AvgTransparency = snapshotEntity.AvgTransparency,
                AvgSafetyTrust = snapshotEntity.AvgSafetyTrust,
                RatingCount = snapshotEntity.RatingCount,
                ComputedAt = snapshotEntity.ComputedAt,
                ConfidenceScore = CalculateConfidenceScore(snapshotEntity.RatingCount)
            };

            return Ok(response);
        }

        [HttpPut("{domain}/ratings")]
        [Authorize]
        public async Task<IActionResult> SubmitRating(string domain, [FromBody] CreateRatingRequest ratingRequest)
        {

            _logger.LogInformation("Testing logs for {Domain}", domain);

            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prefer the OpenID Connect "sub" (subject) claim
                var userIdString =
                    User.FindFirst(Claims.Subject)?.Value               // OpenIddict/OIDC "sub"
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Fallback

                // if (string.IsNullOrWhiteSpace(userId))
                //     return Forbid(); // or Unauthorized() depending on your flow
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                    return Unauthorized(new { message = "User not authenticated or invalid ID format." });

                // Prevents duplicates by normalized domain
                var normalizedDomain = DomainUtility.NormalizeDomain(domain);
                if (string.IsNullOrEmpty(normalizedDomain))
                    return BadRequest(new { message = "Invalid URL provided." });

                // Rating upsert creates website if missing
                var website = await _websiteRepository.GetByNormalizedDomainAsync(normalizedDomain);

                // 1. The Business Logic: Auto-create website if missing
            if (website == null)
            {
                var defaultCategory = await _categoryRepository.GetByNameAsync("Uncategorized");
                
                // If it doesn't exist, we rely on the repository to handle creation, 
                // or we create it here and save.
                if (defaultCategory == null)
                {
                    defaultCategory = new Category { Name = "Uncategorized", Slug = "uncategorized", IsActive = true };
                    await _categoryRepository.AddAsync(defaultCategory);
                    await _categoryRepository.SaveChangesAsync(); // Assuming a SaveChanges method exists
                }

                website = new Website
                {
                    Domain = normalizedDomain,
                    Name = normalizedDomain,
                    DisplayName = !string.IsNullOrWhiteSpace(ratingRequest.DisplayName) 
                        ? ratingRequest.DisplayName 
                        : normalizedDomain,
                    CategoryId = defaultCategory.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _websiteRepository.AddAsync(website);
                await _websiteRepository.SaveChangesAsync(); // MUST save to get the ID
            }

                // 2. Prepare the Domain Entity
                var rating = new RatingEntity
                {
                    UserId = userId,
                    // Website = website, // Link the whole entity for EF Core to handle the FK
                    WebsiteId = website.Id,
                    Accuracy = ratingRequest.Accuracy,
                    BiasNeutrality = ratingRequest.BiasNeutrality,
                    Transparency = ratingRequest.Transparency,
                    SafetyTrust = ratingRequest.SafetyTrust,
                    Comment = ratingRequest.Comment
                };

                // 3. The Repository handles the DB, the Calculator handles the math
                var snapshotEntity = await _ratingRepository.UpsertRatingAsync(rating);

                // 6. Map the Domain Entity -> API Contract (DTO) to send back to Angular
                var response = new UpdatedSnapshotResponse
                {
                    WebsiteId = snapshotEntity.WebsiteId,
                    Score0to100 = snapshotEntity.Score,
                    AvgAccuracy = snapshotEntity.AvgAccuracy,
                    AvgBiasNeutrality = snapshotEntity.AvgBiasNeutrality,
                    AvgTransparency = snapshotEntity.AvgTransparency,
                    AvgSafetyTrust = snapshotEntity.AvgSafetyTrust,
                    RatingCount = snapshotEntity.RatingCount,
                    ComputedAt = snapshotEntity.ComputedAt,
                    ConfidenceScore = CalculateConfidenceScore(snapshotEntity.RatingCount) // Confidence
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // THIS IS THE MAGIC! It will print the exact database crash to your browser
                return StatusCode(500, new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
                
            }

            // Find existing rating for for "userId" + "ratingRequest.WebsiteId"
        }
    }
}