using Microsoft.AspNetCore.Identity;

namespace CredibilityIndex.Infrastructure.Auth
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}