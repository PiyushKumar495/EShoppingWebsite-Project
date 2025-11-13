import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-payments',
  standalone: true,
  imports: [MaterialModule, CommonModule],
  templateUrl: './admin-payments.component.html',
  styleUrls: ['./admin-payments.component.css']
})
export class AdminPaymentsComponent implements OnInit {
  payments: any[] = [];
  loading = true;
  error = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    const token = localStorage.getItem('authToken'); // Retrieve the token
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.get<any[]>('/api/Admin/all-payments', { headers }).subscribe({
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
