// app.routes.ts
import { inject } from '@angular/core';
import { Routes } from '@angular/router';

import { SearchPageComponent } from './features/search-page/search-page.component';
import { WebsiteDetailsComponent } from './features/website-details/website-details.component';
import { RateWebsiteComponent } from './features/rate-website/rate-website.component';
import { MyRatingsComponent } from './features/my-ratings/my-ratings.component';
import { AdminCategoriesComponent } from './features/admin-categories/admin-categories.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AdminDashboardComponent } from './features/admin-dashboard/admin-dashboard.component';
import { AuthCallbackComponent } from './features/auth/callback/callback.component';

import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';
import { AuthService } from './core/auth/auth.service';


export const routes: Routes = [
    { path: '', pathMatch: 'full', component: SearchPageComponent },
    { path: 'website/:domain', component: WebsiteDetailsComponent },
    {
        path: 'rate/:domain',
        component: RateWebsiteComponent,
        canActivate: [authGuard]
    },
    {
        path: 'admin/categories',
        component: AdminCategoriesComponent,
        canActivate: [adminGuard]
    },
    {
        path: 'me/ratings',
        component: MyRatingsComponent,
        canActivate: [authGuard]
    },
    { path: 'auth/register', component: RegisterComponent },
    { path: 'admin', component: AdminDashboardComponent, canActivate: [adminGuard] },

    // OAuth/OIDC redirect handler
    { path: 'callback', component: AuthCallbackComponent },

    // Convenience route: visiting /login starts the OIDC flow
    {
        path: 'login',
        canActivate: [() => { inject(AuthService).login('/'); return false; }],
        children: []
    },

    { path: '**', redirectTo: '' },
];