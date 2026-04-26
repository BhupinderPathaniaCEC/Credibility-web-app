// using CredibilityIndex.Application.DTOs;
// using CredibilityIndex.Application.Interfaces;
// using CredibilityIndex.Domain.Entities;

// namespace CredibilityIndex.Application.Services;

// public class AuthService
// {
//     private readonly IUserRepository _userRepository;

//     public AuthService(IUserRepository userRepository)
//     {
//         _userRepository = userRepository;
//     }

//     public async Task RegisterUser(RegisterRequest request)
//     {
//         // 1. Business Rule: Check for duplicates
//         if (await _userRepository.ExistsAsync(request.Email))
//             throw new Exception("User already exists");

//         // 2. Map DTO to Entity
//         var user = new UserEntity 
//         { 
//             Email = request.Email, 
//             Username = request.Username,
//             PasswordHash = "hashed_password_here" // You'll add hashing later
//         };

//         await _userRepository.AddAsync(user);
//     }
// }