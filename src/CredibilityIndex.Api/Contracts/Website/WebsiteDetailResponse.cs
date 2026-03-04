namespace CredibilityIndex.Api.Contracts.Website;

public class WebsiteDetailResponse
{
    // Metadata
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Domain { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public CategoryDto Category { get; set; } = null!;

    // Score & Breakdown (The Snapshot)
    public CredibilitySnapshotDto? Snapshot { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class CredibilitySnapshotDto
{
    public byte Score { get; set; }
    public double AvgAccuracy { get; set; }
    public double AvgBiasNeutrality { get; set; }
    public double AvgTransparency { get; set; }
    public double AvgSafetyTrust { get; set; }
    public int RatingCount { get; set; }
    public DateTime LastUpdated { get; set; }
}