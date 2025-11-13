import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';

interface Payment {
  paymentId: number;
  orderId: number;
  mode: string;
  amount: number;
  paymentDate: string;
  status: string;
}

@Component({
  selector: 'app-payments',
  templateUrl: './payments.component.html',
  styleUrl: './payments.component.css',
  standalone: true,
  imports: [CommonModule, MatProgressBarModule]
})
export class PaymentsComponent implements OnInit {
  payments: Payment[] = [];
  loading = true;
  error = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<Payment[]>('/api/Payment/all-payments').subscribe({
      next: (payments) => {
        this.payments = payments;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load payments.';
        this.loading = false;
      }
    });
  }
}
