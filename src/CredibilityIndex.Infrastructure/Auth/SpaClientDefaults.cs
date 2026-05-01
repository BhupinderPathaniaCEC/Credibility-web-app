namespace CredibilityIndex.Infrastructure.Auth;

/// <summary>
/// Default SPA client URLs used when no <c>SpaClient</c> configuration section
/// is provided (typical local dev). Production environments should override
/// these via configuration — see <see cref="SpaClientOptions"/>.
/// </summary>
internal static class SpaClientDefaults
{
    private const string SpaHttp = "http://localhost:4200";
    private const string SpaHttps = "https://localhost:4200";

    public static readonly string[] RedirectUris =
    {
        $"{SpaHttp}/",
        $"{SpaHttp}/callback",
        $"{SpaHttp}/silent-renew.html",
        $"{SpaHttps}/",
        $"{SpaHttps}/callback",
        $"{SpaHttps}/silent-renew.html"
    };

    public static readonly string[] PostLogoutRedirectUris =
    {
        $"{SpaHttp}/",
        $"{SpaHttps}/"
    };
}
