using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CredibilityIndex.Infrastructure.Persistence;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;

namespace CredibilityIndex.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the content root dynamically to the API project directory
        // Navigate from test assembly location: .../tests/bin/Debug/net10.0 -> .../src/CredibilityIndex.Api
        var testAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
        var contentRoot = Path.GetFullPath(Path.Combine(testAssemblyLocation, "..", "..", "..", "..", "src", "CredibilityIndex.Api"));
        builder.UseContentRoot(contentRoot);

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