using System;
using System.ComponentModel.DataAnnotations;

namespace CredibilityIndex.Domain.Entities
{
    public class RatingEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WebsiteId { get; set; }

        [Required]
        public Guid UserId { get; set; } // Matches diagram as Guid

        // The 4 Dimensions from your diagram
        [Range(1, 5)]
        public int Accuracy { get; set; }

        [Range(1, 5)]
        public int BiasNeutrality { get; set; }

        [Range(1, 5)]
        public int Transparency { get; set; }

        [Range(1, 5)]
        public int SafetyTrust { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}