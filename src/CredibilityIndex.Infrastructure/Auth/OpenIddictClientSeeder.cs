using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
public static class OpenIddictClientSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "mvp-client",
            ClientSecret = "super-secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            DisplayName = "MVP Client",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess
            }
        };

        var existing = await manager.FindByClientIdAsync(descriptor.ClientId);
        if (existing is null)
        {
            await manager.CreateAsync(descriptor);
            return;
        }

        await manager.UpdateAsync(existing, descriptor);
    }
}
