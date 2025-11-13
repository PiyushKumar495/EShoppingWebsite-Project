import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MaterialModule } from '../material.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage-product',
  templateUrl: './manage-product.component.html',
  styleUrls: ['./manage-product.component.css'],
  standalone: true,
  imports: [MaterialModule, ReactiveFormsModule, CommonModule, FormsModule]
})
export class ManageProductComponent implements OnInit {
  productForm: FormGroup;
  updateForm: FormGroup;
  products: any[] = [];
  categories: any[] = [];
  loading = false;
  message = '';
  private apiUrl = '/api/product';
  selectedProductId: number | null = null; // Track the selected product ID for deletion
  deleteProductName: string = '';

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      price: [0, [Validators.required, Validators.min(0)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      categoryName: ['', Validators.required] // Use categoryName instead of categoryId
    });
    this.updateForm = this.fb.group({
      oldName: ['', Validators.required],
      name: ['', Validators.required],
      description: [''],
      price: [0, [Validators.required, Validators.min(0)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      categoryName: ['', Validators.required] // Use categoryName instead of categoryId
    });
    this.fetchProducts();
  }

  ngOnInit() {
    this.fetchCategories();
  }

  createProduct() {
    if (this.productForm.invalid) return;
    this.loading = true;
    // Send the form value directly, backend expects categoryName, not categoryId
    const payload = {
      ...this.productForm.value
    };
    this.http.post(this.apiUrl, payload).subscribe({
      next: (res: any) => {
        this.message = 'Product created!';
        this.productForm.reset();
        this.fetchProducts();
        this.loading = false;
      },
      error: err => {
        this.message = err.error?.Message || 'Failed to create product.';
        this.loading = false;
      }
    });
  }

  updateProduct() {
    if (this.updateForm.invalid) return;
    this.loading = true;
    // Find categoryId by categoryName
    const cat = this.categories.find(c => c.categoryName === this.updateForm.value.categoryName);
    if (!cat) {
      this.message = 'Invalid category name.';
      this.loading = false;
      return;
    }
    const payload = {
      name: this.updateForm.value.name,
      description: this.updateForm.value.description,
      price: this.updateForm.value.price,
      stockQuantity: this.updateForm.value.stockQuantity,
      categoryId: cat.categoryId
    };
    this.http.put(`${this.apiUrl}/by-name?name=${this.updateForm.value.oldName}`, payload).subscribe({
      next: () => {
        this.message = 'Product updated!';
        this.updateForm.reset();
        this.fetchProducts();
        this.loading = false;
      },
      error: err => {
        this.message = err.error?.Message || 'Failed to update product.';
        this.loading = false;
      }
    });
  }

  deleteProduct() {
    // Delete by product ID from the selected product in the All Products grid
    const productId = this.selectedProductId;
    if (!productId) {
      this.message = 'Select a product to delete.';
      return;
    }
    this.loading = true;
    this.http.delete(`${this.apiUrl}/${productId}`).subscribe({
      next: () => {
        this.message = 'Product deleted!';
        this.selectedProductId = null;
        this.fetchProducts();
        this.loading = false;
      },
      error: err => {
        this.message = err.error?.Message || 'Failed to delete product.';
        this.loading = false;
      }
    });
  }

  deleteProductByName() {
    if (!this.deleteProductName) return;
    this.loading = true;
    this.http.delete(`${this.apiUrl}/by-name?name=${encodeURIComponent(this.deleteProductName)}`).subscribe({
      next: () => {
        this.message = 'Product deleted!';
        this.deleteProductName = '';
        this.fetchProducts();
        this.loading = false;
      },
      error: err => {
        this.message = err.error?.Message || 'Failed to delete product.';
        this.loading = false;
      }
    });
  }

  fetchProducts() {
    this.http.get(this.apiUrl).subscribe({
      next: (res: any) => {
        this.products = Array.isArray(res) ? res : [];
      },
      error: () => {
        this.products = [];
      }
    });
  }

  fetchCategories() {
    this.http.get<any[]>('/api/category').subscribe({
      next: (res: any) => {
        this.categories = res;
      },
      error: err => {
        this.categories = [];
      }
    });
  }
}
