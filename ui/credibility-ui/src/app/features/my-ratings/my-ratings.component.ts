import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core'; // 1. Add Inject and PLATFORM_ID
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http'; // We import HTTP directly here!

// 1. We define the exact shape of the C# data right here at the top
export interface MyRating {
  websiteId: number;
  domain: string;
  accuracy: number;
  biasNeutrality: number;
  transparency: number;
  safetyTrust: number;
  personalAverageScore: number;
  currentGlobalScore?: number;
}

@Component({
  selector: 'app-my-ratings',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-ratings.component.html',
  styleUrls: ['./my-ratings.component.css']
})
export class MyRatingsComponent implements OnInit {
  ratings: MyRating[] = [];
  isLoading = true;
  
  // Use relative URL for proxy
  private apiUrl = '/api/v1/me/ratings'; 

  // 2. We inject HttpClient directly into the constructor
  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadMyRatings();
    }
  }

  // 3. The Bouncer Helper: Grabs your OpenIddict token from local storage
  private getHeaders() {
    // Safely check if we are in a browser before calling localStorage
    let token = '';
    if (typeof window !== 'undefined') {
      token = localStorage.getItem('access_token') || '';
    }
    
    return { headers: new HttpHeaders().set('Authorization', `Bearer ${token}`) };
  }

  // 4. The API Call: Reaches out to C# directly from the component
  loadMyRatings(): void {
    this.http.get<MyRating[]>(this.apiUrl, this.getHeaders()).subscribe({
      next: (data) => {
        this.ratings = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching ratings', err);
        this.isLoading = false;
      }
    });
  }

  // --- UI Navigation Buttons ---
  editRating(domain: string): void {
    this.router.navigate(['/rate', domain]);
  }

  viewWebsite(domain: string): void {
    this.router.navigate(['/website', domain]);
  }
}