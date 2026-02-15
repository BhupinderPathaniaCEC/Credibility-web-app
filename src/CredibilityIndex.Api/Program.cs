using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;

using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Infrastructure.Persistence;
using CredibilityIndex.Infrastructure.Auth;
using Microsoft.OpenApi.Models;


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
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<CredibilityDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.SetAuthorizationEndpointUris("/connect/authorize");

        options.AllowAuthorizationCodeFlow()
               .AllowClientCredentialsFlow()
               .AllowPasswordFlow()
               .AllowRefreshTokenFlow();

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// -------------------------
// 4. Authentication
// -------------------------
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

// -------------------------
// 5. Controllers
// -------------------------
builder.Services.AddControllers();

// -------------------------
// 6. Swagger / Swashbuckle
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Credibility API", Version = "v1" });

    // OAuth2 / OpenIddict setup
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                TokenUrl = new Uri("https://localhost:5001/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect scope" }
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "openid" }
        }
    });
});

// -------------------------
// 7. Dependency Injection
// -------------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();

// -------------------------
// Build App
// -------------------------
var app = builder.Build();

// -------------------------
// Swagger UI Middleware
// -------------------------
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Credibility API v1");

        // OAuth2 / PKCE configuration for OpenIddict
        c.OAuthClientId("your-client-id");             // Replace with your OpenIddict client ID
        c.OAuthAppName("Credibility API Swagger UI"); // Display name in Swagger
        c.OAuthUsePkce();                             // PKCE is recommended for security
        c.OAuthScopeSeparator(" ");                   // Space-separated scopes
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant(); // Optional for some OpenIddict setups
    });
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
app.MapDefaultControllerRoute();

app.Run();
