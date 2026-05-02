import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

// 1. Updated Interface to match your exact backend JSON
interface BackendWebsiteResponse {
  id: number;
  name: string;
  domain: string;
  description?: string;
  isActive: boolean;
  category: { id: number; name: string };
  snapshot: {
    score: number;
    avgAccuracy: number;
    avgBiasNeutrality: number;
    avgTransparency: number;
    avgSafetyTrust: number;
    ratingCount: number;
    lastUpdated: string;
  };
}

@Component({
  selector: 'app-website-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './website-details.component.html',
  styleUrls: ['./website-details.component.css']
})
export class WebsiteDetailsComponent implements OnInit {
  domain = '';

  snapshot: any = null;
  
  // 2. We separate the data for the UI
  websiteData: BackendWebsiteResponse | null = null;
  confidenceScore: number = 0; 
  recentRatings: any[] = [];
  
  loading = true;
  error: string | null = null;
  notFound = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.domain = params.get('domain') || '';
      if (this.domain) {
        this.fetchCredibility();
      }
    });
  }

  fetchCredibility(): void {
    this.loading = true;
    this.error = null;
    this.notFound = false;

    this.http.get<BackendWebsiteResponse>(`${environment.apiUrl}/api/v1/websites/${this.domain}/`).subscribe({
      next: (res) => {
        console.log('[DEBUG] SUCCESS! Data received:', res); 
        
        this.websiteData = res;
        this.snapshot = res.snapshot;
        
        const count = res.snapshot?.ratingCount || 0;
        this.confidenceScore = Math.round((count / (count + 10)) * 100);

        this.loading = false;
        
        // ADD THIS LINE: Force the HTML to update instantly!
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        console.error('[DEBUG] ERROR!', err);
        this.loading = false;
        this.error = 'Failed to load credibility data.';
        this.cdr.detectChanges(); // Add it here too just in case!
      }
    });
  }

  rateSite(): void {
    this.router.navigate(['/rate', this.domain]);
  }

  getBarWidth(score: number): string {
    const safeScore = Math.max(0, Math.min(5, score));
    return `${(safeScore / 5) * 100}%`;
  }

  // 1. Calculates the average of the 4 categories for a single review
  getRatingAverage(item: any): number {
    if (!item) return 0;
    const sum = (item.accuracy || 0) + (item.biasNeutrality || 0) + (item.transparency || 0) + (item.safetyTrust || 0);
    return sum / 4;
  }

  // 2. Converts a numeric score (like 4) into a star string (★★★★☆)
  getStarRating(rating: number): string {
    const rounded = Math.round(rating);
    // Ensure the number is between 0 and 5
    const safeRating = Math.max(0, Math.min(5, rounded)); 
    return '★'.repeat(safeRating) + '☆'.repeat(5 - safeRating);
  }

  // 3. Formats the raw backend date into a clean string (e.g., "Mar 17, 2026")
  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}