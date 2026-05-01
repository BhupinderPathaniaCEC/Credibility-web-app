import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * @deprecated Backwards-compat shim for the old TokenService API.
 * New code should inject AuthService directly.
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly auth = inject(AuthService);

  get isLoggedIn$(): Observable<boolean> {
    return this.auth.isLoggedIn$;
  }

  silentlyFetchToken(): void {
    /* no-op: AuthService.init() handles initial token retrieval */
  }

  fetchAndStoreToken(): void {
    /* no-op: kept for backwards compatibility */
  }

  login(): void {
    this.auth.login();
  }

  logout(): void {
    this.auth.logout();
  }

  isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  get accessToken(): string | null {
    return this.auth.accessToken;
  }
}