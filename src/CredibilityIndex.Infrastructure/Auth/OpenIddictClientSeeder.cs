using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Identity;
using CredibilityIndex.Infrastructure.Auth;
public static class OpenIddictClientSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (await roleManager.FindByNameAsync(role) == null)
            {
                try
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
                catch (Exception)
                {
                    // Ignore if role already exists
                }
            }
        }
        var adminEmail = "admin@credibility.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "System Admin",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "mvp-client",
            ClientSecret = "super-secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            DisplayName = "MVP Client",
            Permissions =
            {
                // Endpoints
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                // Grant types
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.GrantTypes.Password,

                // Scopes
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess
            },
            Requirements =
            {
                // Require Proof Key for Code Exchange (PKCE) for public clients like SPAs.
                OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
            }
        };

        var existing = await manager.FindByClientIdAsync(descriptor.ClientId);
        if (existing is null)
        {
            try
            {
                await manager.CreateAsync(descriptor);
                return;
            }
            catch (Exception)
            {
                // If create fails, try to find and update
                existing = await manager.FindByClientIdAsync(descriptor.ClientId);
            }
        }

        if (existing != null)
        {
            await manager.UpdateAsync(existing, descriptor);
        }

        // Register Angular SPA as a public client for authorization code/PKCE flows.
        var spaDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "credibility-ui-spa",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            DisplayName = "Credibility Angular SPA",
            RedirectUris =
            {
                // Served directly by the API at the application root in development.
                new Uri("https://localhost:7222/"),
                new Uri("https://localhost:7222/callback")
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://localhost:7222/")
            },
            Permissions =
            {
                // Endpoints
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                // Grant types
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.GrantTypes.Password,

                // Scopes
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess
            },
            Requirements =
            {
                OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
            }
        };

        var existingSpa = await manager.FindByClientIdAsync(spaDescriptor.ClientId);
        if (existingSpa is null)
        {
            try
            {
                await manager.CreateAsync(spaDescriptor);
            }
            catch (Exception)
            {
                // Ignore if already exists
            }
        }
        else
        {
            await manager.UpdateAsync(existingSpa, spaDescriptor);
        }
    }
}
