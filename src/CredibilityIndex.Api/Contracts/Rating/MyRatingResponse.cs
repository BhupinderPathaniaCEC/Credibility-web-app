namespace CredibilityIndex.Api.Contracts.Rating
{
    public class MyRatingResponse
    {
        public int WebsiteId { get; set; }
        public string Domain { get; set; } = string.Empty;
        public int Accuracy { get; set; }
        public int BiasNeutrality { get; set; }
        public int Transparency { get; set; }
        public int SafetyTrust { get; set; }
        public double PersonalAverageScore { get; set; } 
        public int? CurrentGlobalScore { get; set; } 
    }
}