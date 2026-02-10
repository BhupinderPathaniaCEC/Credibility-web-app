using Microsoft.AspNetCore.Mvc;
using CredibilityIndex.Api.Contracts;

namespace CredibilityIndex.Api.Controllers;

[ApiController]
[Route("api/v1/auth")] // Matches your openapi.yml path
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        // 1. Validation logic would go here
        if (string.IsNullOrEmpty(request.Email)) return BadRequest("Email required");

        // 2. For now, we simulate a successful registration
        // In the next step, you will move this logic to the Application layer
        var response = new RegisterResponse(Guid.NewGuid(), request.Email, request.DisplayName);

        return CreatedAtAction(nameof(Register), response);
    }
}