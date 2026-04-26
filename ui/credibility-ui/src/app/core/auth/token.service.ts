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
  isAdmin(): boolean {
    // 1. Grab the token (Change 'access_token' if you named it something else!)
    const token = localStorage.getItem('access_token'); 
    
    if (!token) return false;

    try {
      // 2. JWTs have 3 parts separated by dots. The data (payload) is the middle part [1].
      const payloadBase64Url = token.split('.')[1];
      
      // 3. Convert it to standard Base64, then decode it into a readable JSON string
      const payloadBase64 = payloadBase64Url.replace(/-/g, '+').replace(/_/g, '/');
      const payloadJson = JSON.parse(atob(payloadBase64));

      // 4. Look for the Role claim. 
      // (.NET sometimes uses a long schema URL for roles, or just the word 'role')
      const roles = payloadJson.role || payloadJson['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      if (!roles) return false;

      // 5. If the user has multiple roles, it's an array. If they have one, it's a string.
      if (Array.isArray(roles)) {
        return roles.includes('Admin');
      } else {
        return roles === 'Admin';
      }
      
    } catch (error) {
      console.error('Error decoding token:', error);
      return false; // If the token is fake or broken, assume they are not an admin
    }
  }
}
