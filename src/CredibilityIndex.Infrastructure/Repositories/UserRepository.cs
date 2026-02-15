using CredibilityIndex.Application.Interfaces;
using CredibilityIndex.Domain.Entities;
using Microsoft.EntityFrameworkCore; // This is required for AnyAsync and ToListAsync   

namespace CredibilityIndex.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    // Static list so data persists as long as the app is running

    private readonly CredibilityDbContext _context;

    public UserRepository(CredibilityDbContext context)
    {
        _context = context;
    }
    // private static readonly List<UserEntity> _users = new();

    public async Task AddAsync(UserEntity user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsAsync(string email)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        return exists;
    }
} 