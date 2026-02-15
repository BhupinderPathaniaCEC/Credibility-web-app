using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(UserEntity user);
    Task<bool> ExistsAsync(string email);
}