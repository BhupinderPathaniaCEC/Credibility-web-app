// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
// using CredibilityIndex.Infrastructure.Persistence; // Ensure this matches your namespace

// namespace CredibilityIndex.Infrastructure.Persistence;

// public class CredibilityDbContextFactory : IDesignTimeDbContextFactory<CredibilityDbContext>
// {
//     public CredibilityDbContext CreateDbContext(string[] args)
//     {
//         var optionsBuilder = new DbContextOptionsBuilder<CredibilityDbContext>();
        
//         // This is only used by the "dotnet ef" commands to understand your schema
//         optionsBuilder.UseSqlite("Data Source=design_time.db"); 

//         return new CredibilityDbContext(optionsBuilder.Options);
//     }
// }