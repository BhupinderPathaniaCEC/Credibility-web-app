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

        // GET: api/auth/token - Issue a token for the currently authenticated user (via Identity cookie)
        [HttpPost("token")]
        [Authorize(AuthenticationSchemes = "Identity.Application")] // Requires the Identity Cookie
        public async Task<IActionResult> GetToken()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // 1. Create the Identity specifically for OpenIddict
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            // 2. Add your Claims
            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, user.Email)
                    .SetClaim(Claims.Name, user.UserName)
                    .SetClaim(Claims.GivenName, user.DisplayName ?? user.UserName);

            var roles = await _userManager.GetRolesAsync(user);
            identity.SetClaims(Claims.Role, [.. roles]);

            // 3. Create the Principal and set the Scopes requested by your Angular app
            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(Scopes.Profile, Scopes.Email, Scopes.OfflineAccess);

            // 4. THE MOST IMPORTANT STEP: Set Destinations
            // By default, OpenIddict keeps claims hidden for security. 
            // You MUST tell it to put these claims inside the Access Token.
            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(Destinations.AccessToken);
            }

            // 5. THE MAGIC: Return a SignIn result targeting the OpenIddict scheme.
            // OpenIddict will intercept this, grab your X509 certificate, sign the token, 
            // apply the 60-minute expiration, and return the standard JSON response!
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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

            _logger.LogInformation("Token exchange requested. grant_type={GrantType}, client_id={ClientId}", request.GrantType, request.ClientId);

            if (request.IsRefreshTokenGrantType())
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
                {
                    return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return SignIn(authenticateResult.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (!request.IsPasswordGrantType())
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var user = await _userManager.FindByEmailAsync(request.Username)
                       ?? await _userManager.FindByNameAsync(request.Username);

            if (user is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var valid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!valid.Succeeded)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, nameType: Claims.Name, roleType: Claims.Role);
            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user));
            identity.SetClaim(Claims.Email, user.Email);
            identity.SetClaim(Claims.Name, user.UserName);
            identity.SetClaim(Claims.GivenName, user.DisplayName ?? user.UserName);

            var roles = await _userManager.GetRolesAsync(user);
            identity.SetClaims(Claims.Role, [.. roles]);

            // Set proper destinations for each claim
            identity.SetDestinations(claim => claim.Type switch
            {
                Claims.Subject or Claims.Email or Claims.Name or Claims.GivenName or Claims.Role
                    => new[] { Destinations.AccessToken, Destinations.IdentityToken },
                _ => new[] { Destinations.AccessToken }
            });

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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