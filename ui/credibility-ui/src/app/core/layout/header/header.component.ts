import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http'; 
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
  isLoggedIn = false;
  private readonly API_BASE = 'https://localhost:7222'; // Centralize your URL

  constructor(private router: Router, private http: HttpClient) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.checkLocalToken();
    });
  }

  ngOnInit() {
    this.checkLocalToken();
    this.silentlyFetchTokenFromCookie();
  }

  private checkLocalToken() {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('access_token');
      this.isLoggedIn = !!token;
    }
  }

  private silentlyFetchTokenFromCookie() {
    if (this.isLoggedIn) return;

    this.http.get<{ access_token: string }>(`${this.API_BASE}/api/v1/auth/token`, { withCredentials: true })
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

  /**
   * REVISED LOGOUT LOGIC
   * 1. Revokes JWT on server
   * 2. Clears local storage
   * 3. Redirects to Identity UI to clear Cookie
   */
  logout() {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('access_token');

      if (token) {
        // OpenIddict revocation expects x-www-form-urlencoded data
        const body = new HttpParams()
          .set('token', token)
          // Ensure this ClientId matches what you registered in Program.cs
          .set('client_id', 'angular_client'); 

        const headers = new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded');

        this.http.post(`${this.API_BASE}/connect/revoke`, body.toString(), { headers })
          .subscribe({
            next: () => this.finalizeLogout(),
            error: (err) => {
              console.error('Revocation failed or already revoked:', err);
              this.finalizeLogout(); // Continue logout even if server call fails
            }
          });
      } else {
        this.finalizeLogout();
      }
    }
  }

  private finalizeLogout() {
    localStorage.removeItem('access_token');
    this.isLoggedIn = false;

    // This kills the Identity.Application cookie
    window.location.href = `${this.API_BASE}/Identity/Account/Logout`;
  }
}