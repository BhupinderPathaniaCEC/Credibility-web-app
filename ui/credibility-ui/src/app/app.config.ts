import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideAppInitializer, inject } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideOAuthClient } from 'angular-oauth2-oidc';

import { routes } from './app.routes';
import { AuthService } from './core/auth/auth.service';
import { environment } from './environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptorsFromDi()),
    provideOAuthClient({
      // Only allow our API origin to receive the access token
      resourceServer: {
        allowedUrls: ['/api/', environment.apiUrl],
        sendAccessToken: true
      }
    }),
    // Initialize OAuth before the app renders
    provideAppInitializer(() => inject(AuthService).init())
  ]
};
