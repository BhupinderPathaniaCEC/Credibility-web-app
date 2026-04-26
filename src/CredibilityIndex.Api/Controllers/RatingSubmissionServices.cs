// using Microsoft.AspNetCore.Mvc;
// using CredibilityIndex.Application.Common;
// using CredibilityIndex.Application.Interfaces;
// using CredibilityIndex.Domain.Entities;
// using CredibilityIndex.Api.Contracts.Website;

// namespace CredibilityIndex.Api.Controllers;

// [ApiController]
// [Route("api/v1/[controller]")]
// public class RatingsController : ControllerBase
// {
//     // 1. Inject the Application Interface directly (No separate Service needed)
//     private readonly IWebsiteRepository _websiteRepository;

//     public RatingsController(IWebsiteRepository websiteRepository)
//     {
//         _websiteRepository = websiteRepository;
//     }

//     [HttpPost("prepare-website")]
//     public async Task<IActionResult> PrepareWebsiteForRating([FromBody] string rawUrl)
//     {
//         // 2. Normalize the input using your Application logic
//         var normalizedDomain = DomainUtility.NormalizeDomain(rawUrl);

//         if (string.IsNullOrEmpty(normalizedDomain))
//             return BadRequest(new { message = "Invalid domain provided." });

//         // 3. Check the Infrastructure (Database) via the Interface
//         var existingWebsite = await _websiteRepository.GetByNormalizedDomainAsync(normalizedDomain);

//         Website targetWebsite;

//         if (existingWebsite != null)
//         {
//             // The site exists, use it
//             targetWebsite = existingWebsite;
//         }
//         else
//         {
//             // The site is missing, auto-create it with minimal metadata
//             targetWebsite = new Website
//             {
//                 Domain = normalizedDomain,
//                 Name = normalizedDomain, // Defaulting Name to Domain
//                 CategoryId = 1,          // Fallback Category ID
//                 IsActive = true
//             };

//             await _websiteRepository.AddAsync(targetWebsite);
//         }

//         // 4. Map the raw Entity to your safe DTO
//         var responseDto = new AutoCreateWebsite // TODO: No need for AutoCreateWebsite, just use Webiste DTO
//         {
//             Id = targetWebsite.Id,
//             Domain = targetWebsite.Domain, // bbc.com
//             Name = targetWebsite.Name // bbc.com
//         };

//         // 5. Return the clean JSON to Angular
//         return Ok(responseDto);
//     }
// }