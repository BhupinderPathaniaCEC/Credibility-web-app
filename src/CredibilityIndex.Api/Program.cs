using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using OpenIddict.Validation.AspNetCore;

using System.Security.Cryptography.X509Certificates;
using CredibilityIndex.Api.Configuration;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Infrastructure.Persistence;
using CredibilityIndex.Infrastructure.Repositories;
using CredibilityIndex.Infrastructure.Auth;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Logging with Serilog (Structured JSON with CorrelationId enrichment)
// -------------------------
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/log-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// -------------------------
// 1. EF Core + SQLite/SQL Server + OpenIddict entities
// -------------------------
builder.Services.AddDbContext<CredibilityDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.UseInMemoryDatabase("TestDatabase");
    }
    else
    {
        var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=credibility.db";

        if (defaultConnection.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
            (defaultConnection.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) &&
             !defaultConnection.Contains(".db", StringComparison.OrdinalIgnoreCase)))
        {
            options.UseSqlServer(defaultConnection);
        }
        else
        {
            options.UseSqlite(defaultConnection);
        }
    }

    options.UseOpenIddict();
});

// -------------------------
// 2. Identity + Identity UI (Razor Pages serve /Identity/Account/Login)
// -------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CredibilityDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Cookie scheme used to authenticate the user against the Identity UI before
// /connect/authorize issues an authorization code.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = AppUrls.Routes.IdentityLogin;
    options.LogoutPath = AppUrls.Routes.IdentityLogout;
    options.AccessDeniedPath = AppUrls.Routes.IdentityAccessDenied;
});

// -------------------------
// 3. CORS — allow the Angular SPA origin to call the API
// -------------------------
var spaOrigins = builder.Configuration
        .GetSection(AppUrls.ConfigKeys.AllowedCorsOrigins)
        .Get<string[]>()
    ?? AppUrls.DefaultSpaOrigins;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(spaOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// -------------------------
// 4. OpenIddict Server + Validation
// -------------------------
var accessTokenLifetime = builder.Configuration.GetValue<int>("IdentitySettings:AccessTokenLifetimeMinutes");

X509Certificate2? cert = null;
if (!builder.Environment.IsEnvironment("Testing"))
{
    cert = X509CertificateLoader.LoadPkcs12FromFile(
        "openiddict-cert.pfx",
        "SuperSecretPassword123!",
        X509KeyStorageFlags.DefaultKeySet,
        Pkcs12LoaderLimits.Defaults);
    builder.Services.AddSingleton(cert);
}
else
{
    using (var rsa = System.Security.Cryptography.RSA.Create(2048))
    {
        var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(
            "CN=Test", rsa,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
    }
    builder.Services.AddSingleton(cert);
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                   .UseDbContext<CredibilityDbContext>();
        })
        .AddServer(options =>
        {
            // Endpoints
            options.SetAuthorizationEndpointUris(AppUrls.Routes.Authorize);
            options.SetTokenEndpointUris(AppUrls.Routes.Token);
            options.SetEndSessionEndpointUris(AppUrls.Routes.EndSession);
            options.SetRevocationEndpointUris(AppUrls.Routes.Revocation);
            options.SetUserInfoEndpointUris(AppUrls.Routes.UserInfo);

            // Grant types: SPA uses authorization_code + PKCE + refresh_token.
            // Password grant is kept for legacy/testing only.
            options.AllowAuthorizationCodeFlow();
            options.AllowRefreshTokenFlow();
            options.AllowPasswordFlow();

            options.RequireProofKeyForCodeExchange();
            options.AcceptAnonymousClients();

            options.AddEncryptionCertificate(cert)
                   .AddSigningCertificate(cert);

            // SPA-friendly: opaque reference tokens persisted in OpenIddictTokens.
            options.DisableAccessTokenEncryption();
            options.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenLifetime));
            options.UseReferenceAccessTokens();
            options.UseReferenceRefreshTokens();

            options.UseAspNetCore()
                .EnableAuthorizationEndpointPassthrough()
                .EnableTokenEndpointPassthrough()
                .EnableEndSessionEndpointPassthrough()
                .EnableUserInfoEndpointPassthrough()
                .DisableTransportSecurityRequirement();

            options.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.Roles, Scopes.OfflineAccess);
        })
        .AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        });

    // API requests are validated as bearer tokens. The /connect/authorize
    // controller explicitly challenges the Identity cookie scheme to redirect
    // anonymous users to /Identity/Account/Login.
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    });
}

builder.Services.AddAuthorization();

// Bind SPA client options (RedirectUris / PostLogoutRedirectUris / ClientId)
// from the "SpaClient" config section so production hosts can be set via
// appsettings.{Env}.json or environment variables (e.g. SpaClient__RedirectUris__0).
builder.Services.Configure<SpaClientOptions>(
    builder.Configuration.GetSection(SpaClientOptions.SectionName));

// -------------------------
// 5. Controllers + Razor Pages (Identity UI only)
// -------------------------
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// -------------------------
// 6. Swagger
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

// -------------------------
// 7. Application services
// -------------------------
builder.Services.AddScoped<IWebsiteRepository, WebsiteRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IRatingQueryRepository, RatingQueryRepository>();

// -------------------------
// Build app
// -------------------------
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<CredibilityDbContext>();
        await db.Database.MigrateAsync();
        await OpenIddictClientSeeder.SeedAsync(services);
        await CategorySeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------
// Middleware pipeline
// -------------------------
app.UseMiddleware<CorrelationIdMiddleware>();

// Only redirect to HTTPS in production; allow HTTP in development for proxy access
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

// Static files needed by the Razor-rendered Identity UI (CSS, JS, images).
app.UseStaticFiles();

// -------------------------
// Endpoints
// -------------------------
app.MapControllers().RequireCors("AllowAngular");
app.MapRazorPages(); // /Identity/Account/Login, /Identity/Account/Logout, etc.

app.Run();
