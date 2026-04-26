import { Routes } from '@angular/router';
import { SearchPageComponent } from './search-page/search-page.component';

export const routes: Routes = [
	{
		path: '',
		pathMatch: 'full',
		component: SearchPageComponent
	}
];
