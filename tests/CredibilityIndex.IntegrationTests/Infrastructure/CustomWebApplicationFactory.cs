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
            // Override the connection string - not needed for in-memory, but ensures clean state
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["OpenIddict:DisableTokenValidation"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            // CRITICAL: Remove ALL existing DbContext registrations to prevent "multiple providers" error
            // Remove DbContextOptions and DbContext service descriptors
            var dbContextDescriptors = services
                .Where(d => 
                    d.ServiceType == typeof(DbContextOptions<CredibilityDbContext>) ||
                    d.ServiceType == typeof(CredibilityDbContext) ||
                    (d.ServiceType.IsGenericType && 
                     d.ServiceType.Name.Contains("DbContextFactory")))
                .ToList();
            
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Register in-memory database for testing (clean slate)
            services.AddDbContext<CredibilityDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.UseOpenIddict();
            });

            // Override authentication to use test scheme
            var authDescriptors = services
                .Where(d => d.ServiceType == typeof(AuthenticationSchemeOptions))
                .ToList();
            
            foreach (var descriptor in authDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication(TestAuthHandler.TestScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, null);

            // Initialize database schema and seed if needed
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CredibilityDbContext>();
                db.Database.EnsureCreated();
                db.SaveChanges();
            }
        });
    }
}