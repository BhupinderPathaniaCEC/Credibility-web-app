using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography.X509Certificates;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using CredibilityIndex.Infrastructure.Auth;
using CredibilityIndex.Api.Contracts.Auth;
using Microsoft.IdentityModel.Tokens;

namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthController> logger,
            X509Certificate2 signingCertificate = null)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _signingCertificate = signingCertificate;
        }

        private readonly X509Certificate2 _signingCertificate;

        
        // POST: api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)

        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                DisplayName = dto.DisplayName
            };

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
            {
                return BadRequest("User already exists.");
            }

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(e.Code ?? "Identity", e.Description);

                return ValidationProblem(ModelState);
            }

            return Ok(new RegisterResponse(
                user.Id,
                user.Email ?? dto.Email,
                user.DisplayName));
        }

       
        // GET: api/auth/me - Get current user info (requires valid token)
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(Claims.Subject)?.Value;
            var email = User.FindFirst(Claims.Email)?.Value;
            var name = User.FindFirst(Claims.Name)?.Value;
            var roles = User.FindAll(Claims.Role);

            return Ok(new
            {
                UserId = userId,
                Email = email,
                Name = name,
                Roles = roles.Select(c => c.Value).ToList()
            });
        }
    }
}