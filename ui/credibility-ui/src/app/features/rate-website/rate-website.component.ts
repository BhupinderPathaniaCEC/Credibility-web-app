import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-rate-website',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], 
  templateUrl: './rate-website.component.html',
  styleUrls: ['./rate-website.component.css']
})
export class RateWebsiteComponent implements OnInit {
  ratingForm!: FormGroup;
  domain: string = '';
  loading: boolean = true;
  isSubmitting: boolean = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  // Helper array to generate the 1-5 buttons in HTML
  ratingScale = [1, 2, 3, 4, 5];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) { }

  ngOnInit(): void {
    this.domain = this.route.snapshot.paramMap.get('domain') || '';

    // Initialize the form with required validators (1-5 scale)
    this.ratingForm = this.fb.group({
      displayName: [''],
      accuracy: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      bias: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      transparency: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      safety: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: [''] // Optional
    });
    this.loading = false;
    // this.fetchExistingRating();
  }

  // Check if the user already rated this site
  fetchExistingRating(): void {
    const token = localStorage.getItem('access_token');
    if (!token) {
      this.loading = false;  // Guard: no token = skip fetch
      return;
    }

    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    const encodedDomain = encodeURIComponent(this.domain);

    console.log('[DEBUG] Sending request to fetch rating...');
    // FIXED: Use encodedDomain in the URL
    this.http.get<any>(`https://localhost:7222/api/v1/websites/${encodedDomain}/ratings/me`, { headers })
      .subscribe({
        next: (existingRating) => {
          console.log('[DEBUG] API returned:', existingRating);
          if (existingRating) {
            this.ratingForm.patchValue({
              accuracy: existingRating.accuracy,
              bias: existingRating.biasNeutrality,
              transparency: existingRating.transparency,
              safety: existingRating.safetyTrust,
              comment: existingRating.comment
            });
          }
          this.loading = false;
        },
        error: (err) => {
          console.error('[DEBUG] API Error caught:', err);
          if (err.status !== 404) {
            this.errorMessage = 'Could not load previous rating, but you can still submit a new one!';
          }
          this.loading = false;
        },
        complete: () => {
          console.log('[DEBUG] Request finished! Turning off loading screen.');
          // THIS IS THE BULLETPROOF FIX. 
          // It forces the loading screen to vanish when the 204 finishes.
          this.loading = false; 
        }
      });
  }

  // Click handler for the custom rating buttons
  setRating(metric: string, value: number): void {
    this.ratingForm.get(metric)?.setValue(value);
  }

  submitRating(): void {
    if (this.ratingForm.invalid) {
      this.ratingForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;

    const token = localStorage.getItem('access_token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    const encodedDomain = encodeURIComponent(this.domain);

    const raw = this.ratingForm.value;
    const payload = {
      displayName: raw.displayName,
      accuracy: raw.accuracy,
      biasNeutrality: raw.bias,
      transparency: raw.transparency,
      safetyTrust: raw.safety,
      comment: raw.comment
    };

    // FIXED: Use encodedDomain in the URL
    this.http.put(`https://localhost:7222/api/v1/websites/${encodedDomain}/ratings`, payload, { headers })
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          this.successMessage = 'Your rating has been successfully saved!';

          // Route back to the details page after 2 seconds
          setTimeout(() => {
            this.router.navigate(['/website', this.domain]);
          }, 2000);
        },
        error: (err) => {
          console.error('[DEBUG] Save Rating Error caught:', err);
          
          // FIXED: Use the correct error handling for a PUT request
          this.isSubmitting = false; 
          this.errorMessage = 'Failed to submit your rating. Please try again.';
        },
        complete: () => {
          console.log('[DEBUG] Request finished! Turning off loading screen.');
          // THIS IS THE BULLETPROOF FIX. 
          // It forces the loading screen to vanish when the 204 finishes.
          this.loading = false; 
        }
      });
  }

  cancel(): void {
    this.router.navigate(['/website', this.domain]);
  }
}