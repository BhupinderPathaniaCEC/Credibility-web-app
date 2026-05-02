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
  private discoveryLoaded = false;

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
      this.discoveryLoaded = true;
    } catch (err) {
      console.error('[Auth] Discovery / login failed:', err);
    }

    this.loggedIn$.next(this.hasValidToken());

    // Keep the BehaviorSubject in sync with token events
    this.oauth.events.subscribe(() => this.loggedIn$.next(this.hasValidToken()));
  }

  /** Begin the auth code + PKCE flow. Redirects to /connect/authorize. */
  login(targetRoute?: string): void {
    const target = targetRoute ?? window.location.pathname;

    if (this.discoveryLoaded) {
      this.oauth.initCodeFlow(target);
      return;
    }

    this.oauth.loadDiscoveryDocument()
      .then(() => {
        this.discoveryLoaded = true;
        this.oauth.initCodeFlow(target);
      })
      .catch((err) => {
        console.error('[Auth] Login fallback triggered:', err);

        // Keep flow OAuth-only: direct Identity login skips OIDC callback and
        // can bounce back to the API host instead of the SPA callback URI.
      });
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

  getLoginTarget(): string {
    return this.oauth.state || '/';
  }

  isAdmin(): boolean {
    const claims = this.identityClaims;
    if (!claims) return false;
    const roles = claims['role'] ?? claims['roles'];
    if (!roles) return false;
    return Array.isArray(roles) ? roles.includes('Admin') : roles === 'Admin';
  }
}
