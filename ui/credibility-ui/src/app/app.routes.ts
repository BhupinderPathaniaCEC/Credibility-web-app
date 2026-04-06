// Example updated paths in app.routes.ts
import { SearchPageComponent } from './features/search-page/search-page.component';
import { WebsiteDetailsComponent } from './features/website-details/website-details.component';
import { RateWebsiteComponent } from './features/rate-website/rate-website.component';
import { MyRatingsComponent } from './features/my-ratings/my-ratings.component';
import { AdminCategoriesComponent } from './features/admin-categories/admin-categories.component';
import { RegisterComponent } from './features/auth/register/register.component';

import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';
import { Routes } from '@angular/router';
import { AdminDashboardComponent } from './features/admin-dashboard/admin-dashboard.component';


export const routes: Routes = [
    { path: '', pathMatch: 'full', component: SearchPageComponent },
    { path: 'website/:domain', component: WebsiteDetailsComponent },
    { path: 'rate/:domain', component: RateWebsiteComponent },
    { 
    path: 'admin/categories', 
    component: AdminCategoriesComponent,
    canActivate: [adminGuard] // <--- This acts as the bouncer!
  },
  { 
    path: 'me/ratings', 
    component: MyRatingsComponent,
    canActivate: [authGuard] // The new generic guard!
  },
  { path: 'auth/register', component: RegisterComponent },
  { path: 'admin', component: AdminDashboardComponent },
  { path: '**', redirectTo: '' },
];