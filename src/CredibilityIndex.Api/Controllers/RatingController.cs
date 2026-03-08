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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitRating([FromBody] CreateRatingRequest ratingRequest)
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
                
            // 2. ACCEPTANCE CRITERIA: Prevents duplicates by normalized domain
            var normalizedDomain = DomainUtility.NormalizeDomain(ratingRequest.RawUrl);
            if (string.IsNullOrEmpty(normalizedDomain))
                return BadRequest(new { message = "Invalid URL provided." });

            // 3. ACCEPTANCE CRITERIA: Rating upsert creates website if missing
            var website = await _websiteRepository.GetByNormalizedDomainAsync(normalizedDomain);
            
            if (website == null)
            {
                // ACCEPTANCE CRITERIA: Minimal metadata stored
                website = new Website 
                {
                    Domain = normalizedDomain,
                    Name = normalizedDomain, // Defaulting Name to Domain
                    // Note: Your ER diagram uses display_name, so we map it here
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
                WebsiteId = ratingRequest.WebsiteId,
                Accuracy = ratingRequest.Accuracy,
                BiasNeutrality = ratingRequest.BiasNeutrality,
                Transparency = ratingRequest.Transparency,
                SafetyTrust = ratingRequest.SafetyTrust,
                Comment = ratingRequest.Comment
            };

            // 3. The Repository handles the "If exists, update. If not, insert" logic
            await _ratingRepository.UpsertRatingAsync(rating);

            // 4. Return proper DTO
            var averageScore = await _ratingRepository.GetAverageCredibilityAsync(ratingRequest.WebsiteId);

            return Ok(new CreateRatingResponse
            {
                Message = "Rating processed successfully.",
                WebsiteId = ratingRequest.WebsiteId,
                AverageScore = averageScore,
                ComputedAt = DateTime.UtcNow
            });

            // Find existing rating for for "userId" + "ratingRequest.WebsiteId"

            /*
            var rating = new Rating {
                UserId = userId,
                WebsiteId = ratingRequest.WebsiteId,
                ...
                ...
                CreatedAt = DateTime.Now()
            }
            */


            // return CreatedAtAction({ });
        }
    }
}