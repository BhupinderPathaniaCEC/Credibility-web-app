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

namespace CredibilityIndex.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Set environment to "Testing" to disable HTTPS and certificate loading
        builder.UseEnvironment("Testing");
        
        // Walk up from bin/Debug/net10.0 (3 levels) + tests/CredibilityIndex.IntegrationTests (2 levels)
        // = 5 levels up from AppContext.BaseDirectory to reach repo root
        var apiContentRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, 
            "../../../../../src/CredibilityIndex.Api")
        );
        
        builder.UseContentRoot(apiContentRoot);
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override the connection string - using in-memory database for tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                // Ensure OpenIddict token validation is disabled in tests
                ["OpenIddict:DisableTokenValidation"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<CredibilityDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register in-memory database for testing
            services.AddDbContext<CredibilityDbContext>(options =>
            {
                // Use in-memory database so tests don't depend on file system
                options.UseInMemoryDatabase("TestDatabase");
                options.UseOpenIddict();
            });

            // Override authentication to use test scheme instead of OpenIddict
            var authDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(AuthenticationSchemeOptions));
            if (authDescriptor != null)
            {
                services.Remove(authDescriptor);
            }

            // Remove OpenIddict validation and add test authentication
            services.AddAuthentication(TestAuthHandler.TestScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, null);

            // Initialize database with test data
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CredibilityDbContext>();
                db.Database.EnsureCreated(); // Create in-memory database schema
                db.SaveChanges();
            }
        });
    }
}