
namespace CredibilityIndex.Api.Contracts.Category
{
    using System.ComponentModel.DataAnnotations;

    public class CreateCategoryRequest
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public required string Name { get; set; }

        [StringLength(30, MinimumLength = 3)]
        [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must be lowercase and may contain numbers and hyphens.")]
        public string? Slug { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}