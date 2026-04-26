using System;
using System.ComponentModel.DataAnnotations;

namespace CredibilityIndex.Api.Contracts.Rating
{
    public class CreateRatingRequest
    {
        public string RawUrl { get; set; } = string.Empty; 
        
        public int WebsiteId { get; set; }

        [Range(1, 5, ErrorMessage = "Accuracy score must be between 1 and 5.")]
        public int Accuracy { get; set; }

        [Range(1, 5, ErrorMessage = "Transparency score must be between 1 and 5.")]
        public int Transparency { get; set; }

        [Range(1, 5, ErrorMessage = "BiasNeutrality score must be between 1 and 5.")]
        public int BiasNeutrality { get; set; }

        [Range(1, 5, ErrorMessage = "SafetyTrust score must be between 1 and 5.")]
        public int SafetyTrust { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string Comment { get; set; } = "";
    }
}