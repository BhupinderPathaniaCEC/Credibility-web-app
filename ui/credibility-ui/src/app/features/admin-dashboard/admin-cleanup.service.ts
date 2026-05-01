import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, timer } from 'rxjs';
import { switchMap, finalize } from 'rxjs/operators';

// --- TYPES & MODELS ---
export interface Website { id: string; domain: string; categoryId: string; categoryName: string; }
export interface Category { id: string; name: string; }
export interface CleanupState { 
  status: 'pending' | 'validating' | 'success' | 'error'; 
  message?: string; 
}

@Injectable({ providedIn: 'root' })
export class AdminCleanupService {
  // --- CONFIGURATION ---
  private readonly API = '/api/v1';
  private stateStore = new BehaviorSubject<Map<string, CleanupState>>(new Map());
  public states$ = this.stateStore.asObservable();

  constructor(private http: HttpClient) {}

  getWebsites() { return this.http.get<Website[]>(`${this.API}/admin/websites`); }
  getCategories() { return this.http.get<Category[]>(`${this.API}/categories`); }

  /**
   * MINIMAL CLEANUP LOGIC:
   * Chained Validation and Update with automatic UI state management
   */
  processCategoryChange(siteId: string, catId: string): Observable<any> {
    this.updateState(siteId, { status: 'validating', message: 'Updating...' });

    return this.http.get(`${this.API}/categories/${catId}`).pipe(
      switchMap(() => this.http.put(`${this.API}/admin/websites/${siteId}/category`, { 
        newCategoryId: parseInt(catId) 
      })),
      tap({
        next: () => {
          this.updateState(siteId, { status: 'success', message: 'Applied!' });
          // Auto-remove success badge after 3 seconds
          timer(3000).subscribe(() => this.clearState(siteId));
        },
        error: (err) => {
          this.updateState(siteId, { status: 'error', message: err.error?.message || 'Failed' });
        }
      })
    );
  }

  // --- STATE HELPERS ---
  private updateState(id: string, state: CleanupState) {
    const map = new Map(this.stateStore.value);
    map.set(id, state);
    this.stateStore.next(map);
  }

  clearState(id: string) {
    const map = new Map(this.stateStore.value);
    map.delete(id);
    this.stateStore.next(map);
  }
}