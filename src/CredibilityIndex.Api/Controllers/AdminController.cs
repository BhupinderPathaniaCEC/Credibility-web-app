using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using CredibilityIndex.Infrastructure.Auth;
// ONLY import your Application Interfaces here!
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Api.Contracts.Category;

namespace CredibilityIndex.Api.Controllers
{
    public class UpdateWebsiteCategoryRequest
    {
        public int NewCategoryId { get; set; }
    }

    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebsiteRepository _websiteRepository;   // Clean Interface
        private readonly ICategoryRepository _categoryRepository; // Clean Interface

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IWebsiteRepository websiteRepository,
            ICategoryRepository categoryRepository)
        {
            _userManager = userManager;
            _websiteRepository = websiteRepository;
            _categoryRepository = categoryRepository;
        }
     
        [HttpGet("stats")]
        public IActionResult GetSystemStats()
        {
            return Ok(new
            {
                TotalUsers = _userManager.Users.Count(),
                ServerStatus = "Healthy",
                LastBackup = DateTime.UtcNow.AddDays(-1)
            });
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.DisplayName,
                    u.EmailConfirmed
                })
                .ToList();

            return Ok(new { TotalCount = users.Count, Users = users });
        }

        // --- THE CLEAN ARCHITECTURE ENDPOINTS ---

        [HttpGet("websites")]
        public async Task<IActionResult> GetAllWebsites()
        {
            // The Repository handles fetching the data and including the Category Name
            var websites = await _websiteRepository.GetAllWithCategoriesAsync();

            var response = websites.Select(w => new 
            {
                w.Id,
                w.Domain,
                CategoryId = w.CategoryId,
                // Because the Repository included the Category, we can safely read its Name!
                CategoryName = w.Category?.Name ?? "Unknown" 
            });

            return Ok(response);
        }

        [HttpPut("websites/{id:int}/category")]
        public async Task<IActionResult> UpdateWebsiteCategory(int id, [FromBody] UpdateWebsiteCategoryRequest request)
        {
            // 1. Ask repository for the website
            var website = await _websiteRepository.GetByIdAsync(id);
            if (website == null) return NotFound(new { message = "Website not found." });

            // 2. Ask repository if category exists
            var category = await _categoryRepository.GetByIdAsync(request.NewCategoryId);
            if (category == null) return BadRequest(new { message = "Invalid Category ID." });

            // 3. Update and save via repository
            website.CategoryId = request.NewCategoryId;
            
            // If your repo requires an explicit update call, do it here:
            // _websiteRepository.Update(website); 
            
            await _websiteRepository.SaveChangesAsync();

            return Ok(new { message = "Website category updated successfully!" });
        }
    }
}