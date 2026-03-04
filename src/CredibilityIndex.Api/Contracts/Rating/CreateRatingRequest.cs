namespace CredibilityIndex.Api.Contracts.Rating;

public class CreateRatingRequest
{
    public Guid WebsiteId { get; set; }
    public int Accuracy { get; set; }
    public int Transparency { get; set; }
    public int BiasNeutrality { get; set; }
    public int SafetyTrust { get; set; }
    public string Comment { get; set; } = "";
}

public class AutoCreateRatingRequest
    {
        // CHANGED: We now accept the raw URL directly from the user's browser
        public string RawUrl { get; set; } = string.Empty; 
        
        public int Accuracy { get; set; }
        public int Transparency { get; set; }
        public int BiasNeutrality { get; set; }
        public int SafetyTrust { get; set; }
        public string Comment { get; set; } = "";
    }