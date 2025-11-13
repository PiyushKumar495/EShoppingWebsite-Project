import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  productId: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  selectedQuantity?: number; // For UI binding
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = '/api/product';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getByName(name: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/by-name?name=${encodeURIComponent(name)}`);
  }

  getByCategoryName(categoryName: string): Observable<any> {
    return this.http.get(`/api/Category/by-name?name=${encodeURIComponent(categoryName)}`);
  }

  create(product: Omit<Product, 'productId' | 'selectedQuantity'>): Observable<any> {
    return this.http.post(this.apiUrl, product);
  }

  updateByName(name: string, product: Partial<Product>): Observable<any> {
    return this.http.put(`${this.apiUrl}/by-name?name=${encodeURIComponent(name)}`, product);
  }

  deleteByName(name: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/by-name?name=${encodeURIComponent(name)}`);
  }
}
