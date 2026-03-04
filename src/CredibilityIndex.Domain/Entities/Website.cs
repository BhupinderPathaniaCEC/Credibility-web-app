using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CredibilityIndex.Domain.Entities
{
    public class Website
    {
        [Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        
        [Column("display_name")] // Tells SQL to use this column name
        public required string Name { get; set; }
        public string Domain { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true; // Default to active

        public Category Category { get; set; } = null!; // Eager Loading

        // ADD THIS LINE: This fixes the "does not contain a definition" error
        public CredibilitySnapshot? CredibilitySnapshot { get; set; } // Eager Loading
        // Public ICollection<Rating> Ratings {get;set;} = new List<Rating>() // Lazy Loading
    }
}