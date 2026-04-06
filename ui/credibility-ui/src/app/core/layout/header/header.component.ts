import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { HttpClient } from '@angular/common/http'; // Import HttpClient
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css' // <-- Make sure this line exists and matches your filename!
})
export class HeaderComponent implements OnInit {
  isLoggedIn = false;

  constructor(private router: Router, private http: HttpClient) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.checkLocalToken();
    });
  }

  ngOnInit() {
    this.checkLocalToken();
    this.silentlyFetchTokenFromCookie(); // Call the bridge when the app loads!
  }

  private checkLocalToken() {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('access_token');
      this.isLoggedIn = !!token;
    }
  }

  // --- THE BRIDGE ---
  private silentlyFetchTokenFromCookie() {
    // If they already have a token, we don't need to ask for a new one
    if (this.isLoggedIn) return;

    // We ask C# for the OpenIddict token. 
    // The browser will automatically attach the Razor Identity Cookie to this request!
    // Add the v1 right here!
    this.http.get<{ access_token: string }>('https://localhost:7222/api/v1/auth/token', { withCredentials: true })
      .subscribe({
        next: (response) => {
          localStorage.setItem('access_token', response.access_token);
          this.isLoggedIn = true;
        },
        error: (err) => {
          console.warn('No valid Razor cookie found, user remains logged out.');
          this.isLoggedIn = false;
        }
      });
  }

  logout() {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('access_token');
    }
    this.isLoggedIn = false;

    // To fully log out, redirect them to the Razor logout page!
    window.location.href = '/Identity/Account/Logout';
  }
}