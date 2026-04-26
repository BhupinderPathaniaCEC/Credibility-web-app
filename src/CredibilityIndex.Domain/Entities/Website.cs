using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CredibilityIndex.Domain.Entities
{
    public class Website
    {
        [Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }

        [Column("display_name")]
        public required string DisplayName { get; set; } // CHANGED: Name to DisplayName

        public required string Domain { get; set; }

        [Column("url_sample")]
        public string? UrlSample { get; set; } // ADDED: Matches ER Diagram

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!; // Eager Loading

        // ADD THIS LINE: This fixes the "does not contain a definition" error
        public CredibilitySnapshot? CredibilitySnapshot { get; set; } // Eager Loading
        // Public ICollection<Rating> Ratings {get;set;} = new List<Rating>() // Lazy Loading
        public List<RatingEntity> Ratings { get; set; } = new List<RatingEntity>(); // lazy loading
    }
}