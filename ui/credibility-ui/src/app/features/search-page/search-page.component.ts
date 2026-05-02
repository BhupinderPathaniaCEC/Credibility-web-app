import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';

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
  imports: [CommonModule, FormsModule, RouterModule],
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
    private readonly router: Router,
    private readonly auth: AuthService) { }

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

    this.http.get<WebsiteSearchResult[]>(`${environment.apiUrl}/api/v1/websites`, {
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
    if (this.auth.hasValidToken()) {
      this.router.navigate(['/rate', result.domain]);
      return;
    }

    this.auth.login(`/rate/${result.domain}`);
  }
}
