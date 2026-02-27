using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Search([FromQuery] string query)
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
                }
            });

            return Ok(response);
        }
    }
}