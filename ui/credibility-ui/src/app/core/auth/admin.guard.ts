import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.hasValidToken()) {
    auth.login(state.url);
    return false;
  }

  if (auth.isAdmin()) {
    return true;
  }

  console.warn('[Security] Unauthorized access attempt to Admin route.');
  router.navigate(['/']);
  return false;
};