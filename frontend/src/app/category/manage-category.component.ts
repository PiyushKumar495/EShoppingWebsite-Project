import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MaterialModule } from '../material.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-manage-category',
  templateUrl: './manage-category.component.html',
  styleUrls: ['./manage-category.component.css'],
  standalone: true,
  imports: [MaterialModule, ReactiveFormsModule, CommonModule, FormsModule]
})
export class ManageCategoryComponent {
  categoryForm: FormGroup;
  updateForm: FormGroup;
  categories: any[] = [];
  loading = false;
  message = '';
  private apiUrl = '/api/category'; // Use relative path for Angular proxy
  deleteCategoryName: string = '';

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.categoryForm = this.fb.group({
      categoryName: ['', Validators.required]
    });
    this.updateForm = this.fb.group({
      oldName: ['', Validators.required],
      categoryName: ['', Validators.required]
    });
    this.fetchCategories();
  }

  createCategory() {
    if (this.categoryForm.invalid) return;
    this.loading = true;
    // Debug: Log token and outgoing request
    const token = localStorage.getItem('eshop_token');
    console.log('JWT token:', token);
    this.http.post(this.apiUrl, this.categoryForm.value, {
      observe: 'response'
    }).subscribe({
      next: (res: any) => {
        // Debug: Log response headers
        console.log('Create category response headers:', res.headers);
        this.message = res.body?.Message || 'Category created!';
        this.categoryForm.reset();
        this.fetchCategories();
        this.loading = false;
      },
      error: err => {
        this.message = err.error?.Message || 'Failed to create category.';
        this.loading = false;
      }
    });
  }

  updateCategory() {
    if (this.updateForm.invalid) return;
    this.loading = true;
    this.http.put(`${this.apiUrl}/by-name?name=${this.updateForm.value.oldName}`, { categoryName: this.updateForm.value.categoryName }).subscribe({
      next: (res: any) => {
        this.message = res?.Message || 'Category updated!';
        this.updateForm.reset();
        this.fetchCategories();
        this.loading = false;
      },
      error: err => {
        this.message = err.error?.Message || 'Failed to update category.';
        this.loading = false;
      }
    });
  }

  deleteCategory() {
    const name = this.updateForm.get('oldName')?.value;
    if (!name) return;
    this.loading = true;
    this.http.delete(`${this.apiUrl}/by-name?name=${name}`).subscribe({
      next: (res: any) => {
        this.message = res?.Message || 'Category deleted!';
        this.updateForm.reset();
        this.fetchCategories();
        this.loading = false;
      },
      error: err => {
        this.message = err.error?.Message || 'Failed to delete category.';
        this.loading = false;
      }
    });
  }

  deleteCategoryByName() {
    if (!this.deleteCategoryName) return;
    this.loading = true;
    this.http.delete(`${this.apiUrl}/by-name?name=${this.deleteCategoryName}`, { observe: 'response' }).subscribe({
      next: (res: any) => {
        // If backend returns 204 No Content, treat as success
        if (res.status === 204) {
          this.message = 'Category deleted!';
        } else if (res.body && (res.body.Message || res.body.message)) {
          this.message = res.body.Message || res.body.message;
        } else {
          this.message = 'Category deleted!';
        }
        this.deleteCategoryName = '';
        this.fetchCategories();
        this.loading = false;
      },
      error: err => {
        // If error is 404, show not found, else show generic error
        if (err.status === 404) {
          this.message = 'Category not found.';
        } else if (err.error && (err.error.Message || err.error.message)) {
          this.message = err.error.Message || err.error.message;
        } else if (err.status === 0) {
          this.message = 'Cannot connect to server. Please check your backend.';
        } else {
          this.message = 'Failed to delete category.';
        }
        this.loading = false;
      }
    });
  }

  fetchCategories() {
    this.http.get(this.apiUrl).subscribe({
      next: (res: any) => {
        this.categories = Array.isArray(res) ? res : [];
      },
      error: () => {
        this.categories = [];
      }
    });
  }
}
