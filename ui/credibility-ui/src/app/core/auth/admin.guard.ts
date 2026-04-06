import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from './token.service'; // Adjust path if needed!

export const adminGuard: CanActivateFn = (route, state) => {
  // Use inject() to grab your services instead of a constructor
  const tokenService = inject(TokenService);
  const router = inject(Router);

  // Ask the token service if this user is an admin
  if (tokenService.isAdmin()) {
    return true; // The door opens, let them in!
  } else {
    // They are not an admin. Kick them back to the home page (or a login page)
    console.warn('[Security] Unauthorized access attempt to Admin route.');
    router.navigate(['/']);
    return false;
  }
};