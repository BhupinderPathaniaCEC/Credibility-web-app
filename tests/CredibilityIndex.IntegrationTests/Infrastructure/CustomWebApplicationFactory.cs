using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using CredibilityIndex.Infrastructure.Persistence;
using CredibilityIndex.IntegrationTests.Infrastructure;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using OpenIddict.Server;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace CredibilityIndex.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
{
    // 1. Keep your DB setup
    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CredibilityDbContext>));
    if (descriptor != null) services.Remove(descriptor);
    services.AddDbContext<CredibilityDbContext>(options => options.UseInMemoryDatabase("TestingDb"));

    // 2. Add the Fake Auth Scheme
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
        options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
    })
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });
});
    }

    private async Task SeedClientAsync(IOpenIddictApplicationManager manager)
    {
        if (await manager.FindByClientIdAsync("mvp-client") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "mvp-client",
                ClientSecret = "super-secret",
                DisplayName = "MVP Client",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                }
            });
        }
    }
}