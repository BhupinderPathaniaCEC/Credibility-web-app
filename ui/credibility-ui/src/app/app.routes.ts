import { Routes } from '@angular/router';
import { SearchPageComponent } from './search-page/search-page.component';
import { WebsiteDetailsComponent } from './website-details/website-details.component'; // Import this
import { RateWebsiteComponent } from './rate-website/rate-website.component';

export const routes: Routes = [
    {
        path: '',
        pathMatch: 'full',
        component: SearchPageComponent
    },
    {
        path: 'website/:domain',
        component: WebsiteDetailsComponent
    },
    {
        path: 'rate/:domain',
        component: RateWebsiteComponent
        // canActivate: [AuthGuard] // Uncomment this line if your AuthGuard is ready to protect the route!
    },
    { path: '**', redirectTo: 'search' }
];