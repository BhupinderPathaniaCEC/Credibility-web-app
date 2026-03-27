import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class TokenService {
  constructor(private http: HttpClient) {}

  /**
   * Fetches a JWT token for the currently authenticated user (via Identity cookie).
   * Stores the token in localStorage under 'access_token'.
   */
  fetchAndStoreToken() {
    this.http.get<any>('/api/auth/token', { withCredentials: true }).subscribe({
      next: (resp) => {
        if (resp && resp.access_token) {
          localStorage.setItem('access_token', resp.access_token);
        }
      },
      error: (err) => {
        console.error('Failed to fetch token', err);
      }
    });
  }
}
