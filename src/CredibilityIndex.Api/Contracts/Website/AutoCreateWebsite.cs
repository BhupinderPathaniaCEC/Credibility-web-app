namespace CredibilityIndex.Api.Contracts.Website;

public class AutoCreateWebsite
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Domain {get; set;}

    public bool IsActive { get; set; } = true;
}