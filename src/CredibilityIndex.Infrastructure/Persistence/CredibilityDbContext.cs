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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseOpenIddict();
    }
}