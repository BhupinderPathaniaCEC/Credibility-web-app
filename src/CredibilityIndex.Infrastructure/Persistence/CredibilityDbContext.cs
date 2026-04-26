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

    public DbSet<RatingEntity> Ratings { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Website> Websites { get; set; }
    // expose snapshot table directly for convenience (used by seeding/tests)
    public DbSet<CredibilitySnapshot> CredibilitySnapshots { get; set; }

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

        modelBuilder.Entity<Website>()
        .HasOne(w => w.CredibilitySnapshot)
        .WithOne(s => s.Website)
        .HasForeignKey<CredibilitySnapshot>(s => s.WebsiteId)
        .OnDelete(DeleteBehavior.Cascade); // If website is deleted, delete snapshot

        // Enforces the one rating per (user, website) (upsert) requirement
       modelBuilder.Entity<RatingEntity>()
        .HasIndex(r => new { r.UserId, r.WebsiteId }).IsUnique();
        
    }
}