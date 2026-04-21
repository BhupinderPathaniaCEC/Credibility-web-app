using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using CredibilityIndex.Infrastructure.Persistence;
using System.Security.Cryptography.X509Certificates;

namespace CredibilityIndex.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
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
            // Override the connection string to use a test SQLite database file
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=/tmp/test_credibility.db"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Ensure database is created and migrated before the app starts
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CredibilityDbContext>();
                db.Database.EnsureDeleted(); // Clean start for tests
                db.Database.Migrate(); // Run migrations
            }
        });
    }
}