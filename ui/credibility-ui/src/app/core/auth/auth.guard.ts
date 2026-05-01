import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Allows the route only if the user holds a valid OpenIddict access token.
 * Otherwise initiates the auth_code+PKCE login flow, returning the user to
 * the originally requested URL after sign-in.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);

  if (auth.hasValidToken()) {
    return true;
  }

  auth.login(state.url);
  return false;
};