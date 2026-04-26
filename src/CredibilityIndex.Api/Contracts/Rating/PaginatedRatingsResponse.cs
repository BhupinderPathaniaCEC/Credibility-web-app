using System;
using System.Collections.Generic;

namespace CredibilityIndex.Api.Contracts.Rating
{
    // 1. The wrapper for the pagination
    public class PaginatedRatingsResponse
    {
        // Using 'required' ensures the compiler knows it will always be constructed!
        public required string Domain { get; set; } 
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<RatingItemResponse> Items { get; set; } = new();
    }

    // 2. The individual item inside the list
    public class RatingItemResponse
    {
        public int Id { get; set; }
        public int Accuracy { get; set; }
        public int BiasNeutrality { get; set; }
        public int Transparency { get; set; }
        public int SafetyTrust { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } 
        public string DisplayName { get; set; } = "Anonymous"; 
    }
}