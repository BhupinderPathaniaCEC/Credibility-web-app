import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface WebsiteCategory {
  id: number;
  name: string;
}

interface WebsiteSnapshotSummary {
  score: number;
  ratingCount: number;
}

interface WebsiteSearchResult {
  id: number;
  name: string;
  domain: string;
  description?: string;
  isActive: boolean;
  category: WebsiteCategory;
  snapshot?: WebsiteSnapshotSummary | null;
}

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-page.component.html',
  styleUrls: ['./search-page.component.css']
})

export class SearchPageComponent {
  rawUrl = '';
  results: WebsiteSearchResult[] = [];
  loading = false;
  error: string | null = null;
  hasSearched = false;

  constructor(private readonly http: HttpClient,
    private readonly router: Router) { }

  get canSearch(): boolean {
    return this.rawUrl.trim().length > 0 && !this.loading;
  }

  search(): void {
    const query = this.rawUrl.trim();
    this.error = null;
    this.hasSearched = true;

    if (!query) {
      this.results = [];
      return;
    }

    this.loading = true;

    this.http
      .get<WebsiteSearchResult[]>(`/api/v1/websites`, {
        params: { query }
      })
      .subscribe({
        next: (res) => {
          this.results = res ?? [];
          this.loading = false;
        },
        error: (err) => {
          console.error('Search error', err);
          this.error = 'Something went wrong while searching. Please try again.';
          this.results = [];
          this.loading = false;
        }
      });
  }

  viewDetails(result: WebsiteSearchResult): void {
    this.router.navigate(['/website', result.domain]);
  }

  rate(result: WebsiteSearchResult): void {
    const ratingURL = encodeURIComponent(`/rate/${encodeURIComponent(result.domain)}`);
    if (localStorage.getItem('access_token')) {
      this.router.navigate(['/rate', result.domain]);
    } else {
      window.location.href = `/Identity/Account/Login?returnUrl=${ratingURL}`;
    }
  }

  createAndRate(): void {
    const current = this.rawUrl.trim();
    if (!current) return; // Don't navigate if empty

    // Encode the website the user typed (e.g., "google.com")
    const encodedUrl = encodeURIComponent(current);

    // The target URL should now be /rate/google.com
    const target = `/rate/${encodedUrl}`;
    const returnUrl = encodeURIComponent(target);

    if (localStorage.getItem('access_token')) {
      // NAVIGATE TO THE ACTUAL DOMAIN, NOT "new"
      this.router.navigate(['/rate', current]);
    } else {
      window.location.href = `/Identity/Account/Login?returnUrl=${returnUrl}`;
    }
  }
}
