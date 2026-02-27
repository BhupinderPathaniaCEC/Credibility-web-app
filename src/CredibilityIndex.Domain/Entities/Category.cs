using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CredibilityIndex.Domain.Entities
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true; // Default to active

        // ADD THIS LINE: This tells EF that one Category has many Websites
        public ICollection<Website> Websites { get; set; } = new List<Website>();
    }
}
/*

Types of database tables relationships:

1:1
Employee (PermanentAddress) --> PermanentAddress (Employee)


1:*
Category (ICollection<Website>) --> Websites (Category)



*/