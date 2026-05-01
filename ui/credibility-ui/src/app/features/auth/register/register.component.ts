import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  private apiUrl = '/api/v1/auth/register';

  constructor(
    private fb: FormBuilder, 
    private http: HttpClient, 
    private router: Router
  ) {
    // 1. Initialize the form with validation rules
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator }); // Attach the custom matching rule
  }

  // 2. Custom validator to ensure passwords match
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    
    if (password !== confirmPassword) {
      control.get('confirmPassword')?.setErrors({ mismatch: true });
      return { mismatch: true };
    } else {
      return null;
    }
  }

  // 3. Handle the submission
  onSubmit() {
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    const payload = {
      email: this.registerForm.value.email,
      displayName: this.registerForm.value.displayName,
      password: this.registerForm.value.password
    };

    this.http.post(this.apiUrl, payload).subscribe({
      next: () => {
        this.isLoading = false;
        // Acceptance Criteria: Redirect to login on success
        this.successMessage = 'Account created! Redirecting to login...';
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 2000); // Wait 2 seconds so they see the success message
      },
      error: (err) => {
        this.isLoading = false;
        // Acceptance Criteria: Handle validation errors from C#
        console.error('Registration error:', err);
        this.errorMessage = err.error?.message || 'Registration failed. Please check your details and try again.';
      }
    });
  }
}