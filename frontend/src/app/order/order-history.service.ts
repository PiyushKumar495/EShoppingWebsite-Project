import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface OrderItem {
  orderItemId: number;
  productId: number;
  productName?: string;
  quantity: number;
  price: number;
}

export interface Order {
  orderId: number;
  orderDate: string;
  totalAmount: number;
  status: string;
  shippingAddress: string;
  paymentMethod: string;
  items: OrderItem[];
  payment?: {
    paymentId: number;
    orderId: number;
    mode: string;
    amount: number;
    paymentDate: string;
    status: string;
  };
}

@Injectable({ providedIn: 'root' })
export class OrderHistoryService {
  private apiUrl = '/api/order';

  constructor(private http: HttpClient) {}

  getMyOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  updateOrderStatus(orderId: number, newStatus: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/admin/update-status/${orderId}`, newStatus, {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
