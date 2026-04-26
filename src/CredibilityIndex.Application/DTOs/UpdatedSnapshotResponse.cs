using System;

namespace CredibilityIndex.Application.DTOs
{
    public class UpdatedSnapshotResponse
    {
        public int WebsiteId { get; set; }
        public double Score0to100 { get; set; }
        public double AvgAccuracy { get; set; }
        public double AvgBiasNeutrality { get; set; }
        public double AvgTransparency { get; set; }
        public double AvgSafetyTrust { get; set; }
        public int RatingCount { get; set; }
        public DateTime ComputedAt { get; set; }
    }
}