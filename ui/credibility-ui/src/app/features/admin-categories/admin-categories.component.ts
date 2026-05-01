import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-categories.component.html',
  styleUrls: ['./admin-categories.component.css']
})
export class AdminCategoriesComponent implements OnInit {
  categories: any[] = [];
  showForm = false;
  categoryForm: FormGroup;
  private apiUrl = '/api/v1/categories';

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.categoryForm = this.fb.group({
      id: [null],
      name: ['', Validators.required],
      slug: ['', Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  private getHeaders() {
    const token = localStorage.getItem('access_token');
    return { headers: new HttpHeaders().set('Authorization', `Bearer ${token}`) };
  }

  loadCategories(): void {
    this.http.get<any[]>(this.apiUrl, this.getHeaders()).subscribe({
      next: (data) => this.categories = data,
      error: (err) => console.error('Failed to load categories', err)
    });
  }

  saveCategory(): void {
    if (this.categoryForm.invalid) return;

    const data = this.categoryForm.value;

    if (data.id) {
      this.http.put(`${this.apiUrl}/${data.id}`, data, this.getHeaders()).subscribe({
        next: () => {
          this.loadCategories();
          this.toggleAddMode();
        }
      });
    } else {
      this.http.post(this.apiUrl, data, this.getHeaders()).subscribe({
        next: () => {
          this.loadCategories();
          this.toggleAddMode();
        }
      });
    }
  }

  disableCategory(id: number): void {
    this.http.patch(`${this.apiUrl}/${id}/toggle-status`, {}, this.getHeaders()).subscribe({
      next: () => {
        console.log(`Category ${id} status toggled!`);
        this.loadCategories();
      },
      error: (err) => {
        console.error('Failed to toggle category status', err);
      }
    });
  }

  toggleAddMode(): void {
    this.showForm = !this.showForm;
    this.categoryForm.reset({ isActive: true });
  }

  editCategory(category: any): void {
    this.showForm = true;
    this.categoryForm.patchValue(category);
  }
}
