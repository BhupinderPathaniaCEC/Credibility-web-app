using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using CredibilityIndex.Api.Contracts.Category;
using Microsoft.AspNetCore.Authorization;

namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Route("api/v1/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;


        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null || !category.IsActive)
                return NotFound(new { message = "Category not found" });

            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Name = request.Name,
                Slug = request.Slug ?? request.Name.ToLower().Replace(" ", "-"),
                Description = request.Description,
                IsActive = request.IsActive ?? true
            };

            await _categoryRepository.AddAsync(category);

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.IsActive
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            if (!string.IsNullOrWhiteSpace(request.Name))
                category.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Slug))
                category.Slug = request.Slug;

            if (!string.IsNullOrWhiteSpace(request.Description))
                category.Description = request.Description;

            if (request.IsActive.HasValue)
                category.IsActive = request.IsActive.Value;

            await _categoryRepository.UpdateAsync(category);

            return Ok(new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.IsActive
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            if (category.IsActive)
                return BadRequest(new { message = "Category must be inactive before it can be deleted" });

            await _categoryRepository.DeleteAsync(id);

            return Ok(new { message = "Category deleted successfully" });
        }

        [HttpPut("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            try
            {
                var categoryStatus = await _categoryRepository.ToggleStatusAsync(id);
                return Ok(new
                {
                    CategoryId = id,
                    IsActive = categoryStatus,
                    message = $"Category is now {(categoryStatus ? "active" : "inactive")}"
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                    return NotFound(new { message = "Category not found" });
                else
                    return StatusCode(500, new { message = "An error occurred while toggling category status" });
            }
        }
    }
}
