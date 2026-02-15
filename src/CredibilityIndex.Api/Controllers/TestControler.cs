using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // <--- Required for [Authorize]
// ... other usings
using CredibilityIndex.Application.Interfaces;
// using CredibilityIndex.Application.DTOs;
using CredibilityIndex.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace CredibilityIndex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public TestController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // Existing Register and Login methods...

    [HttpGet("secure-data")]
    [Authorize] // This method is now protected
    public IActionResult GetSecureData()
    {
        // This code only runs if the JWT is valid
        return Ok(new { 
            message = "This is protected information!",
            user = User.Identity?.Name, // Gets email/name from the Token
            timestamp = DateTime.UtcNow 
        });
    }
}