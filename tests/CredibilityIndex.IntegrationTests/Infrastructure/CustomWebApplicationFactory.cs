using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CredibilityIndex.Infrastructure.Persistence;
using System.Security.Cryptography.X509Certificates;

namespace CredibilityIndex.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the content root to the test directory so it can find the certificate
        builder.UseContentRoot("/home/bhupinderpathania/code/Credibility-web-app/tests/CredibilityIndex.IntegrationTests/bin/Debug/net10.0");

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