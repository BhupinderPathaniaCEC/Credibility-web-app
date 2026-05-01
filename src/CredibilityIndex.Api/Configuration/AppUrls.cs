namespace CredibilityIndex.Api.Configuration;

/// <summary>
/// Centralizes well-known URLs and config keys so we don't repeat literals
/// like "https://localhost:7222" or "http://localhost:4200" across the code base.
///
/// Production overrides come from <c>appsettings.{Environment}.json</c> using
/// the keys exposed below.
/// </summary>
public static class AppUrls
{
    /// <summary>API base URL (HTTPS profile in launchSettings).</summary>
    public const string ApiHttps = "https://localhost:7222";

    /// <summary>API base URL (HTTP profile in launchSettings).</summary>
    public const string ApiHttp = "http://localhost:5149";

    /// <summary>Angular dev server (ng serve) — HTTP.</summary>
    public const string SpaHttp = "http://localhost:4200";

    /// <summary>Angular dev server (ng serve) — HTTPS.</summary>
    public const string SpaHttps = "https://localhost:4200";

    /// <summary>Default origins allowed to call the API in development.</summary>
    public static readonly string[] DefaultSpaOrigins = { SpaHttp, SpaHttps };

    public static class ConfigKeys
    {
        public const string AllowedCorsOrigins = "Cors:AllowedOrigins";
    }

    public static class Routes
    {
        public const string Authorize = "/connect/authorize";
        public const string Token = "/connect/token";
        public const string EndSession = "/connect/logout";
        public const string Revocation = "/connect/revoke";
        public const string UserInfo = "/connect/userinfo";

        public const string IdentityLogin = "/Identity/Account/Login";
        public const string IdentityLogout = "/Identity/Account/Logout";
        public const string IdentityAccessDenied = "/Identity/Account/AccessDenied";
    }
}
