import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserPayment {
  paymentId: number;
  orderId: number;
  mode: string;
  amount: number;
  paymentDate: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class UserPaymentService {
  private apiUrl = '/api/User/orders/payments';

  constructor(private http: HttpClient) {}

  getUserPayments(): Observable<{ payments: UserPayment[]; totalPayments: number }> {
    return this.http.get<{ payments: UserPayment[]; totalPayments: number }>(this.apiUrl);
  }

  makePayment(payment: { orderId: number; mode: string; amount: number }): Observable<any> {
    return this.http.post('/api/payment', payment);
  }
}
