import { Injectable, inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { BehaviorSubject, Observable } from 'rxjs';
import { authConfig } from './auth.config';

/**
 * Wraps angular-oauth2-oidc OAuthService to expose a simple API
 * for the rest of the app (login, logout, isLoggedIn$, isAdmin, accessToken).
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oauth = inject(OAuthService);

  private readonly loggedIn$ = new BehaviorSubject<boolean>(false);
  readonly isLoggedIn$: Observable<boolean> = this.loggedIn$.asObservable();

  /** Configure OAuthService and try to log in from the redirect or stored tokens. */
  async init(): Promise<void> {
    this.oauth.configure(authConfig);
    this.oauth.setupAutomaticSilentRefresh();

    // Loads /.well-known/openid-configuration, then handles the
    // ?code=... parameter on /callback if present.
    try {
      await this.oauth.loadDiscoveryDocumentAndTryLogin();
    } catch (err) {
      console.error('[Auth] Discovery / login failed:', err);
    }

    this.loggedIn$.next(this.hasValidToken());

    // Keep the BehaviorSubject in sync with token events
    this.oauth.events.subscribe(() => this.loggedIn$.next(this.hasValidToken()));
  }

  /** Begin the auth code + PKCE flow. Redirects to /connect/authorize. */
  login(targetRoute?: string): void {
    this.oauth.initLoginFlow(targetRoute ?? window.location.pathname);
  }

  /** Trigger the OpenIddict end-session endpoint and clear local tokens. */
  logout(): void {
    this.oauth.revokeTokenAndLogout().catch(() => this.oauth.logOut());
  }

  hasValidToken(): boolean {
    return this.oauth.hasValidAccessToken();
  }

  get accessToken(): string | null {
    return this.oauth.getAccessToken();
  }

  get identityClaims(): Record<string, unknown> | null {
    return (this.oauth.getIdentityClaims() as Record<string, unknown>) ?? null;
  }

  isAdmin(): boolean {
    const claims = this.identityClaims;
    if (!claims) return false;
    const roles = claims['role'] ?? claims['roles'];
    if (!roles) return false;
    return Array.isArray(roles) ? roles.includes('Admin') : roles === 'Admin';
  }
}
