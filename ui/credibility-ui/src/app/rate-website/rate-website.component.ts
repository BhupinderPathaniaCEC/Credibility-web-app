import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms'; // 1. Added ReactiveFormsModule here
import { CommonModule } from '@angular/common'; // 2. Added CommonModule here
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-rate-website',
  standalone: true, // 3. Ensure this is set to true
  imports: [CommonModule, ReactiveFormsModule], // 4. THIS IS THE MAGIC LINE THAT FIXES THE ERROR
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
      accuracy: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      bias: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      transparency: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      safety: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: [''] // Optional
    });

    this.fetchExistingRating();
  }

  // Check if the user already rated this site
  fetchExistingRating(): void {
    // 1. Grab the token from where you saved it during login
    const token = localStorage.getItem('access_token');
    if (!token) {
      this.loading = false;  // ← Guard: no token = skip fetch
      return;
    }
    // 2. Staple it to the Authorization header
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    const encodedDomain = encodeURIComponent(this.domain);

    // 3. Send the request with the headers attached
    this.http.get<any>(`https://localhost:7222/api/v1/websites/${this.domain}/ratings/me`, { headers })
      .subscribe({
        next: (existingRating) => {
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



    this.isSubmitting = true;
    const token = localStorage.getItem('access_token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    // ✅ Encode here too
    const encodedDomain = encodeURIComponent(this.domain);

    const raw = this.ratingForm.value;
    const payload = {
      accuracy: raw.accuracy,
      biasNeutrality: raw.bias,      // ✅ Fix field name mismatch
      transparency: raw.transparency,
      safetyTrust: raw.safety,       // ✅ Fix field name mismatch
      comment: raw.comment
    };
    // Pass the { headers } object as the third argument here
    this.http.put(`https://localhost:7222/api/v1/websites/${this.domain}/ratings`, payload, { headers })
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          this.successMessage = 'Your rating has been successfully saved!';

          // Optional: Route back to the details page after 2 seconds
          setTimeout(() => {
            this.router.navigate(['/website', this.domain]);
          }, 2000);
        },
        error: (err) => {
          console.error('[DEBUG] API Error caught:', err);

          // If it's a 404, it means no rating exists yet, which is totally fine!
          if (err.status !== 404) {
            this.errorMessage = 'Could not load previous rating, but you can still submit a new one!';
          }

          // THIS IS THE MAGIC LINE: It forces the loading screen to vanish!
          this.loading = false;
        }
      });
  }

  cancel(): void {
    this.router.navigate(['/website', this.domain]);
  }
}