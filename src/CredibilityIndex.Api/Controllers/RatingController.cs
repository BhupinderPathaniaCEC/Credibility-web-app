using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Contracts.Rating;
using CredibilityIndex.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Application.Common;


namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Route("api/v1/websites")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IWebsiteRepository _websiteRepository; // NEW: Injecting Website interface

        public RatingController(IRatingRepository ratingRepository, IWebsiteRepository websiteRepository)
        {
            _ratingRepository = ratingRepository;
            _websiteRepository = websiteRepository;
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

            var normalizedDomain = DomainUtility.NormalizeDomain(decodedDomain);

            // You will need to make sure your repository has a method to get a single user's rating by domain
            var existingRating = await _ratingRepository.GetUserRatingForDomainAsync(normalizedDomain, userId);

            if (existingRating == null)
                return NotFound(); // Angular expects this 404 if they haven't rated it yet!

            return Ok(existingRating);
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

            if (website == null)
            {
                website = new Website
                {
                    Domain = normalizedDomain,
                    Name = normalizedDomain, // Defaulting Name to Domain
                    DisplayName = normalizedDomain,
                    CreatedAt = DateTime.UtcNow
                };

                // Save the new website to the database immediately so we can link the rating to it
                await _websiteRepository.AddAsync(website);
            }

            // 2. Prepare the Domain Entity
            var rating = new RatingEntity
            {
                UserId = userId,
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

            // Find existing rating for for "userId" + "ratingRequest.WebsiteId"
        }
    }
}