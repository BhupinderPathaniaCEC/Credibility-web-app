import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('access_token');

  // If they have a token, let them in. 
  // (In a production app, you'd also check if it's expired here)
  if (token) {
    return true; 
  } else {
    // Kick unauthenticated users to the home/login page
    router.navigate(['/']); 
    return false;
  }
};