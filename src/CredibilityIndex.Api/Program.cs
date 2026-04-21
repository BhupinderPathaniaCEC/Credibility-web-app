using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants.GrantTypes;

using System.Security.Cryptography.X509Certificates;
using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Infrastructure.Persistence;
using CredibilityIndex.Infrastructure.Repositories;
using CredibilityIndex.Infrastructure.Auth;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Server.IIS;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Logging with Serilog (Structured JSON with CorrelationId enrichment)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext() // Crucial: This picks up the "CorrelationId" from our middleware
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()) // Structured JSON
    .CreateLogger();

builder.Host.UseSerilog();

// -------------------------
// 1. EF Core + SQLite + OpenIddict entities
// -------------------------
builder.Services.AddDbContext<CredibilityDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict(); // Enable OpenIddict entities
});

// -------------------------
// 2. Identity + UI
// -------------------------

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CredibilityDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Cookie Configuration (For the UI)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LogoutPath = "/Identity/Account/Logout";
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200","https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// -------------------------
// 3. OpenIddict Server + Validation
// -------------------------
/// Access token lifetime is configured in appsettings.json and read here & without change code
var accessTokenLifetime = builder.Configuration.GetValue<int>("IdentitySettings:AccessTokenLifetimeMinutes");

// Only load the certificate in non-Testing environments
X509Certificate2? cert = null;
if (!builder.Environment.IsEnvironment("Testing"))
{
    cert = new X509Certificate2("openiddict-cert.pfx", "SuperSecretPassword123!");
    // Register the signing certificate in DI for JWT manual creation
    builder.Services.AddSingleton(cert);
}

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<CredibilityDbContext>();
    })
        .AddServer(options =>
        {
            // Endpoints
            options.SetAuthorizationEndpointUris("/connect/authorize");
            options.SetTokenEndpointUris("/connect/token");
            options.SetRevocationEndpointUris("/connect/revoke");

            // Grant types
            options.AllowAuthorizationCodeFlow();
            options.AllowRefreshTokenFlow();
            options.AllowPasswordFlow();

            // For SPA/public clients, PKCE is strongly recommended.
            options.RequireProofKeyForCodeExchange();

            // Accept anonymous clients (no confidential client authentication enforced).
            options.AcceptAnonymousClients();

            // Use development certificates in Testing environment
            if (builder.Environment.IsEnvironment("Testing"))
            {
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
            }
            else
            {
                options.AddEncryptionCertificate(cert)
                    .AddSigningCertificate(cert);
            }

            // In production, use a real certificate or other secure method to store keys
            options.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenLifetime));
            options.UseReferenceRefreshTokens();

            options.UseAspNetCore()
                .EnableAuthorizationEndpointPassthrough()
                .EnableTokenEndpointPassthrough()
                .DisableTransportSecurityRequirement();

            options.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.OfflineAccess);
        })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// -------------------------
// 4. Authentication
// -------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization();

// -------------------------
// 5. Controllers + Razor Pages (Identity UI)
// -------------------------
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// -------------------------
// 6. Swagger / Swashbuckle
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Memory Cache
builder.Services.AddMemoryCache();

// -------------------------
// 7. Dependency Injection
// -------------------------
builder.Services.AddScoped<IWebsiteRepository, WebsiteRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IRatingQueryRepository, RatingQueryRepository>();



// Add this in builder.Services section (before var app = builder.Build())
// builder.Services.Configure<IISServerOptions>(options =>
// {
//     options.AllowSynchronousIO = true;
// });

// THIS is the actual fix for dots in route segments:
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap["domainConstraint"] = typeof(string);
});

// -------------------------
// Build App
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
// Middleware
// -------------------------
app.UseMiddleware<CorrelationIdMiddleware>();

// Only use HTTPS redirection when not in Testing environment
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();


// Serve Angular static files from wwwroot/browser as the web root for the SPA
var angularRootPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "browser");
if (Directory.Exists(angularRootPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(angularRootPath),
        RequestPath = string.Empty
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(angularRootPath),
        RequestPath = string.Empty
    });
}
// Serve default static files (includes Identity UI resources like CSS, JS)
app.UseStaticFiles();

// Serve Bootstrap from custom location
var bootstrapPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "lib", "bootstrap", "dist");
if (Directory.Exists(bootstrapPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(bootstrapPath),
        RequestPath = "/Identity/lib/bootstrap/dist"
    });
}

// -------------------------
// Map Controllers & Identity UI (Razor Pages)
// -------------------------
app.MapControllers();
app.MapRazorPages();

// Fallback to SPA index.html for any unmatched routes (including "/").
if (Directory.Exists(angularRootPath))
{
    app.MapFallback(async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(angularRootPath, "index.html"));
    });
}
else
{
    app.MapFallbackToFile("index.html");
}

app.Run();
