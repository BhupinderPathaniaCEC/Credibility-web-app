import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../environment';

/** OpenIddict client identifier — must match OpenIddictClientSeeder.SeedAsync(). */
export const SPA_CLIENT_ID = 'credibility-ui-spa';

/** Scopes requested by the SPA. */
export const SPA_SCOPES = 'openid profile email roles offline_access';

/** Callback paths registered as RedirectUris on the OpenIddict client. */
export const AuthRoutes = {
  callback: '/callback',
  postLogout: '/',
  silentRenew: '/silent-renew.html'
} as const;

/**
 * OpenID Connect / OAuth2 configuration matching the OpenIddict SPA client
 * registered in OpenIddictClientSeeder.SeedAsync().
 *
 * Flow: authorization_code + PKCE (public client, no secret).
 */
export const authConfig: AuthConfig = {
  // OpenIddict issuer (must match exactly; OIDC discovery uses /.well-known/openid-configuration)
  issuer: window.location.origin + environment.apiUrl,

  // Where the SPA is served (registered as a redirect URI in the seeder)
  redirectUri: window.location.origin + AuthRoutes.callback,
  postLogoutRedirectUri: window.location.origin + AuthRoutes.postLogout,
  silentRefreshRedirectUri: window.location.origin + AuthRoutes.silentRenew,

  clientId: SPA_CLIENT_ID,
  responseType: 'code', // Authorization Code Flow + PKCE

  scope: SPA_SCOPES,

  // PKCE is enforced by OpenIddict
  // (angular-oauth2-oidc enables PKCE automatically when responseType='code')

  // Use refresh tokens (offline_access) instead of silent iframe renew
  useSilentRefresh: false,

  showDebugInformation: !environment.production,

  // OpenIddict's discovery uses HTTPS by default; allow http only in dev if needed
  requireHttps: environment.production
};
