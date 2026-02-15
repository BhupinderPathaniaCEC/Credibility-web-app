using CredibilityIndex.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using CredibilityIndex.Domain.Entities;
using OpenIddict.EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CredibilityIndex.Infrastructure.Persistence;

public class CredibilityDbContext : IdentityDbContext<ApplicationUser>
{
    public CredibilityDbContext(DbContextOptions<CredibilityDbContext> options)
        : base(options)
    {
    }

    public new DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserEntity>().ToTable("Users");
        modelBuilder.Entity<UserEntity>().HasKey(u => u.Id);
        modelBuilder.UseOpenIddict();
    }
}