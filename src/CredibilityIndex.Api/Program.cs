using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants.GrantTypes;

using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Infrastructure.Persistence;
using CredibilityIndex.Infrastructure.Repositories;
using CredibilityIndex.Infrastructure.Auth;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.FileProviders;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

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

// -------------------------
// 3. OpenIddict Server + Validation
// -------------------------
/// Access token lifetime is configured in appsettings.json and read here & without change code
var accessTokenLifetime = builder.Configuration.GetValue<int>("IdentitySettings:AccessTokenLifetimeMinutes");
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

         // Grant types
         options.AllowAuthorizationCodeFlow();
         options.AllowRefreshTokenFlow();

         // For SPA/public clients, PKCE is strongly recommended.
         options.RequireProofKeyForCodeExchange();

         // Accept anonymous clients (no confidential client authentication enforced).
         options.AcceptAnonymousClients();

         // Development signing & encryption credentials
         options.AddDevelopmentEncryptionCertificate()
             .AddDevelopmentSigningCertificate();

         // In production, use a real certificate or other secure method to store keys
         options.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenLifetime));

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


// -------------------------
// Build App
// -------------------------
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        await OpenIddictClientSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
    var db = services.GetRequiredService<CredibilityDbContext>();
    await db.Database.MigrateAsync();
    await OpenIddictClientSeeder.SeedAsync(services);
    await CategorySeeder.SeedAsync(services);
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// -------------------------
// Middleware
// -------------------------
app.UseHttpsRedirection();

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
else
{
    // Fallback to default static file handling if the Angular build folder is missing
    app.UseStaticFiles();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

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
