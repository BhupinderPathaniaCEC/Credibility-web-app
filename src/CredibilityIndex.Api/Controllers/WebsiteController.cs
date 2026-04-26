using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Contracts.Website;
using CredibilityIndex.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Route("api/v1/websites")]
    public class WebsitesController : ControllerBase
    {
        private readonly IWebsiteRepository _websiteRepository;

        public WebsitesController(IWebsiteRepository websiteRepository)
        {
            _websiteRepository = websiteRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string? query)
        {
            // The Repository handles normalization and empty checks
            var websites = await _websiteRepository.SearchWebsitesAsync(query);

            // Returning matching list with metadata
            var response = websites.Select(w => new
            {
                w.Id,
                w.Name,
                w.Domain,
                w.Description,
                w.IsActive,
                Category = new
                {
                    w.Category.Id,
                    w.Category.Name
                },
                Snapshot = w.CredibilitySnapshot == null ? null : new
                {
                    w.CredibilitySnapshot.Score,
                    w.CredibilitySnapshot.RatingCount
                }
            });

            return Ok(response);
        }

        // Added endpoint to fetch a single website along with its current credibility snapshot
        [HttpGet("{domain}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWebsiteDetails(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return BadRequest(new { message = "The domain parameter is required." });
            // Decode the URL parameter in case the frontend sends encoded characters (like %2F)
            var decodedDomain = System.Net.WebUtility.UrlDecode(domain);

            var website = await _websiteRepository.GetWebsiteWithSnapshotByDomainAsync(decodedDomain);

            if (website == null)
                return NotFound(new { message = $"No credibility data found for domain: {decodedDomain}",
                suggestion = "Please check the spelling. If the domain is correct, it may not have been rated yet." });

            // Map the Domain Entity to our clean Contract
            var response = new WebsiteDetailResponse
            {
                Id = website.Id,
                Name = website.Name,
                Domain = website.Domain,
                Description = website.Description,
                IsActive = website.IsActive,
                Category = new CategoryDto
                {
                    Id = website.Category.Id,
                    Name = website.Category.Name
                },
                Snapshot = website.CredibilitySnapshot == null ? null : new CredibilitySnapshotDto
                {
                    Score = website.CredibilitySnapshot.Score,
                    AvgAccuracy = website.CredibilitySnapshot.AvgAccuracy,
                    AvgBiasNeutrality = website.CredibilitySnapshot.AvgBiasNeutrality,
                    AvgTransparency = website.CredibilitySnapshot.AvgTransparency,
                    AvgSafetyTrust = website.CredibilitySnapshot.AvgSafetyTrust,
                    RatingCount = website.CredibilitySnapshot.RatingCount,
                    LastUpdated = website.CredibilitySnapshot.ComputedAt
                }
            };

            return Ok(response);
        }
    }
}