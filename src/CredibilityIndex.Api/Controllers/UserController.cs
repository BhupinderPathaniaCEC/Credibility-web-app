using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CredibilityIndex.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;



namespace CredibilityIndex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // This ensures only logged-in users with a valid JWT can access ANY method here
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Behind the scenes: The Middleware extracted the Email from the JWT Claim
        var email = User.FindFirst(Claims.Email)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return Unauthorized("User email not found in token.");

        // Check if user still exists in our 'In-Memory' database
        var userExists = await _userRepository.ExistsAsync(email);
        if (!userExists)
            return NotFound("User not found.");

        return Ok(new
        {
            Email = email,
            FullName = "New User", // You can expand your User entity later to include this
            Role = "Standard User",
            LastLogin = DateTime.UtcNow
        });
    }

    [HttpGet("status")]
    public IActionResult GetSystemStatus()
    {
        return Ok(new { Status = "Online", SecuredAt = DateTime.UtcNow });
    }
}