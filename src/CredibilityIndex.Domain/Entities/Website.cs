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
        public required string Name { get; set; }
        public string Domain { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true; // Default to active

        public Category Category { get; set; } // Eager Loading
        public CredibilitySnapshot Snapshot { get; set; } // Eager Loading
        // Public ICollection<Rating> Ratings {get;set;} = new List<Rating>() // Lazy Loading
    }
}