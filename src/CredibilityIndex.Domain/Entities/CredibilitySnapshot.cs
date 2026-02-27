using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CredibilityIndex.Domain.Entities
{
    public class CredibilitySnapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int WebsiteId { get; set; }
        public byte Score { get; set; }
        public double AvgAccuracy { get; set; }
        public double AvgBiasNeutrality { get; set; }
        public double AvgTransparency { get; set; }
        public double AvgSafetyTrust { get; set; }
        public int RatingCount { get; set; }
        public DateTime ComputedAt { get; set; }

        public Website Website { get; set; }
    }
}

// Flow: When user submit Ratings, then we will calculate snapshot for that website and update this entity.