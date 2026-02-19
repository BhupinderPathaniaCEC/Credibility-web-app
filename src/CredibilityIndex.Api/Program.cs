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
using CredibilityIndex.Infrastructure.Auth;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;


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
// 2. Identity
// -------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CredibilityDbContext>()
    .AddDefaultTokenProviders();

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
        options.SetTokenEndpointUris("/connect/token");
        
        // Register grant types explicitly
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        
        // Accept anonymous clients (allows client_id/client_secret without pre-registration validation)
        options.AcceptAnonymousClients();

        //Development signing & encryption credentials
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();
        
        // In production, use a real certificate or other secure method to store keys
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenLifetime));

        options.UseAspNetCore()
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
// 5. Controllers
// -------------------------
builder.Services.AddControllers();

// -------------------------
// 6. Swagger / Swashbuckle
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// 7. Dependency Injection
// -------------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();



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
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// -------------------------
// Map Controllers
// -------------------------
app.MapControllers();

app.Run();
