using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using CredibilityIndex.Infrastructure.Auth;
using CredibilityIndex.Api.Contracts;

namespace CredibilityIndex.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

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

        // OpenIddict token endpoint (password grant only).
        // Exposed as an MVC action so it shows up in Swagger.
        [HttpPost("~/connect/token")]
        [IgnoreAntiforgeryToken]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.Features.Get<OpenIddictServerAspNetCoreFeature>()?.Transaction?.Request
                ?? throw new InvalidOperationException("The OpenIddict request cannot be retrieved.");

            if (!request.IsPasswordGrantType())
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var user = await _userManager.FindByEmailAsync(request.Username)
                       ?? await _userManager.FindByNameAsync(request.Username);

            if (user is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var valid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!valid.Succeeded)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user));
            identity.SetClaim(Claims.Email, user.Email);
            identity.SetClaim(Claims.Name, user.UserName);
            identity.SetDestinations(_ => new[] { Destinations.AccessToken });

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}