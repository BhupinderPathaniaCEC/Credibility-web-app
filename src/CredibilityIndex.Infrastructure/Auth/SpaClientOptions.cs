namespace CredibilityIndex.Infrastructure.Auth;

/// <summary>
/// SPA OpenID Connect client settings. Bound to the "SpaClient" configuration
/// section so each environment (Development / Staging / Production) overrides
/// hosts via <c>appsettings.{Env}.json</c> or environment variables.
///
/// Example:
/// <code>
/// "SpaClient": {
///   "ClientId": "credibility-ui-spa",
///   "RedirectUris": [
///     "https://app.credibility.example.com/callback",
///     "https://app.credibility.example.com/silent-renew.html"
///   ],
///   "PostLogoutRedirectUris": [ "https://app.credibility.example.com/" ]
/// }
/// </code>
/// </summary>
public sealed class SpaClientOptions
{
    public const string SectionName = "SpaClient";

    public string ClientId { get; set; } = "credibility-ui-spa";

    public string DisplayName { get; set; } = "Credibility Angular SPA";

    public string[] RedirectUris { get; set; } = Array.Empty<string>();

    public string[] PostLogoutRedirectUris { get; set; } = Array.Empty<string>();
}
