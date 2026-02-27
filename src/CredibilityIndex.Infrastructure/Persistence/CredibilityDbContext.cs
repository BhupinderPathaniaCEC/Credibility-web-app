using CredibilityIndex.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using CredibilityIndex.Domain.Entities;
using OpenIddict.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CredibilityIndex.Infrastructure.Persistence;

public class CredibilityDbContext : IdentityDbContext<ApplicationUser>
{
    public CredibilityDbContext(DbContextOptions<CredibilityDbContext> options)
        : base(options)
    {

    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Website> Websites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseOpenIddict();
        modelBuilder.Entity<Website>(entity =>
        {
        // Define the Foreign Key Relationship
        entity.HasOne(w => w.Category)
              .WithMany(c => c.Websites)
              .HasForeignKey(w => w.CategoryId)
              .OnDelete(DeleteBehavior.Restrict); // Prevents accidental deletion of categories

        // Ensure the Normalized Domain is Unique (to prevent duplicate ratings)
        entity.HasIndex(w => w.Domain).IsUnique();
        });
        
    }
}