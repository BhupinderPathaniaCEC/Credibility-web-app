using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CredibilityIndex.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CredibilityIndex.Api.Controllers
{
    /// <summary>
    /// Handles OpenIddict's OIDC endpoints (/connect/authorize, /connect/token, /connect/logout).
    /// Flow:
    ///   SPA -> /connect/authorize
    ///        -> not authenticated => Challenge Identity cookie scheme
    ///        -> redirected to /Identity/Account/Login
    ///        -> back to /connect/authorize with cookie
    ///        -> SignIn with claims principal => OpenIddict issues code/tokens
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthorizationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthorizationController> _logger;
        private readonly X509Certificate2? _signingCertificate;

        public AuthorizationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthorizationController> logger,
            X509Certificate2? signingCertificate = default)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _signingCertificate = signingCertificate;
        }

        /// <summary>Token exchange endpoint for authorization code and refresh token grants.</summary>
        [HttpPost("~/connect/token")]
        [IgnoreAntiforgeryToken]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> Exchange()
        {
            try
            {
                var feature = HttpContext.Features.Get<OpenIddictServerAspNetCoreFeature>();
                var request = feature?.Transaction?.Request;
                if (request == null)
                {
                    _logger?.LogError("OpenIddict request cannot be retrieved from context.");
                    return BadRequest("The OpenIddict request cannot be retrieved.");
                }

                _logger?.LogInformation("Token exchange requested. grant_type={GrantType}, client_id={ClientId}", request.GrantType, request.ClientId);

                if (request.IsRefreshTokenGrantType())
                {
                    var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
                    {
                        _logger?.LogWarning("Refresh token authentication failed.");
                        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }

                    return SignIn(authenticateResult.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                if (request.IsAuthorizationCodeGrantType())
                {
                    var principal = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    if (!principal.Succeeded || principal.Principal is null)
                    {
                        _logger?.LogWarning("Authorization code authentication failed.");
                        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }

                    return SignIn(principal.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                if (request.IsPasswordGrantType())
                {
                    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    {
                        _logger?.LogWarning("Password grant missing username or password.");
                        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }

                    var user = await _userManager.FindByEmailAsync(request.Username)
                               ?? await _userManager.FindByNameAsync(request.Username);

                    if (user is null)
                    {
                        _logger?.LogWarning("User not found for password grant.");
                        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }

                    var valid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                    if (!valid.Succeeded)
                    {
                        _logger?.LogWarning("Password validation failed.");
                        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }

                    var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, nameType: Claims.Name, roleType: Claims.Role);
                    identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user));
                    identity.SetClaim(Claims.Email, user.Email);
                    identity.SetClaim(Claims.Name, user.UserName);
                    identity.SetClaim(Claims.GivenName, user.DisplayName ?? user.UserName);

                    var roles = await _userManager.GetRolesAsync(user);
                    identity.SetClaims(Claims.Role, [.. roles]);

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

                _logger?.LogWarning("Unsupported grant type: {GrantType}", request.GrantType);
                return BadRequest("The specified grant type is not supported.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred during token exchange.");
                return StatusCode(500, "An error occurred during token exchange.");
            }
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.Features.Get<OpenIddictServerAspNetCoreFeature>()?.Transaction?.Request
                ?? throw new InvalidOperationException("The OpenIddict request cannot be retrieved.");

            // Try to authenticate the user via the Identity cookie
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            // OpenIddict's nested constant class for prompt values has been renamed across
            // major versions (Prompts -> PromptValues). Use the OIDC literal strings directly
            // ("login", "none") to stay version-agnostic.
            const string PromptLogin = "login";
            const string PromptNone = "none";

            var promptValues = (request.Prompt ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var promptHasLogin = promptValues.Contains(PromptLogin);
            var promptHasNone = promptValues.Contains(PromptNone);

            if (!result.Succeeded ||
                promptHasLogin ||
                (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
                 DateTimeOffset.UtcNow - result.Properties.IssuedUtc >
                     TimeSpan.FromSeconds(request.MaxAge.Value)))
            {
                // If the client sent prompt=none, fail fast
                if (promptHasNone)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }));
                }

                // Strip prompt=login so the redirect back doesn't loop into another challenge
                var parameters = Request.HasFormContentType
                    ? Request.Form.Where(p => p.Key != Parameters.Prompt).ToList()
                    : Request.Query.Where(p => p.Key != Parameters.Prompt).ToList();

                var remainingPrompts = promptValues.Where(p => p != PromptLogin).ToArray();
                if (remainingPrompts.Length > 0)
                {
                    parameters.Add(new KeyValuePair<string, StringValues>(
                        Parameters.Prompt, new StringValues(string.Join(" ", remainingPrompts))));
                }

                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                    });
            }

            // The user is signed in. Build the claims principal for OpenIddict.
            var user = await _userManager.GetUserAsync(result.Principal!)
                ?? throw new InvalidOperationException("User details cannot be retrieved.");

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                    .SetClaim(Claims.GivenName, user.DisplayName ?? await _userManager.GetUserNameAsync(user));

            var roles = await _userManager.GetRolesAsync(user);
            identity.SetClaims(Claims.Role, roles.ToImmutableArray());

            identity.SetScopes(request.GetScopes());
            identity.SetResources("resource_server");
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/logout")]
        [HttpPost("~/connect/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties { RedirectUri = "/" });
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            switch (claim.Type)
            {
                case Claims.Name:
                case Claims.PreferredUsername:
                case Claims.GivenName:
                    yield return Destinations.AccessToken;
                    if (claim.Subject!.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;
                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;
                    if (claim.Subject!.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;
                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;
                    if (claim.Subject!.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;
                    yield break;

                // Never include the security stamp in tokens
                case "AspNet.Identity.SecurityStamp":
                    yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
