import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface OrderRequest {
  shippingAddress: string;
  paymentMethod: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = '/api/order';

  constructor(private http: HttpClient) {}

  placeOrder(data: OrderRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/place`, data);
  }

  cancelOrder(orderId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${orderId}`);
  }
}
