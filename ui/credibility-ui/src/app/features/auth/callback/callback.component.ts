import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

/**
 * Landing route for the OpenIddict /connect/authorize redirect.
 * AuthService.init() (via APP_INITIALIZER) has already processed the
 * authorization code by the time this component activates.
 */
@Component({
  selector: 'app-auth-callback',
  standalone: true,
  template: '<p>Signing you in...</p>'
})
export class AuthCallbackComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    // angular-oauth2-oidc stores the original route in `state`
    const target = this.auth['oauth']?.state || '/';
    this.router.navigateByUrl(decodeURIComponent(target as string) || '/', { replaceUrl: true });
  }
}
