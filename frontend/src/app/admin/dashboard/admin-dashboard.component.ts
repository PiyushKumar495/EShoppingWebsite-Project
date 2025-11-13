import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MaterialModule } from '../../material.module';
import { MatTableModule } from '@angular/material/table';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';


interface OrderItem {
  orderItemId: number;
  productId: number;
  productName?: string;
  quantity: number;
  price: number;
}
interface Order {
  orderId: number;
  orderDate: string;
  totalAmount: number;
  status: string;
  shippingAddress: string;
  paymentMethod: string;
  userName?: string;
  items?: OrderItem[];
}

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css'],
  standalone: true,
  imports: [MaterialModule, MatTableModule, MatProgressBarModule, MatSelectModule, MatFormFieldModule]
})
export class AdminDashboardComponent implements OnInit {
  totalOrders = 0;
  totalPayments = 0;
  orders: Order[] = [];
  loading = false;
  error: string | null = null;
  expandedOrderId: number | null = null;

  toggleOrderDetails(orderId: number) {
    this.expandedOrderId = this.expandedOrderId === orderId ? null : orderId;
  }

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.fetchOrders();
    this.fetchPayments();
  }

  fetchOrders() {
    this.loading = true;
    this.error = null;

    const token = localStorage.getItem('authToken'); // Retrieve the token
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.get<Order[]>('/api/Admin/admin/all-orders', { headers }).subscribe({
      next: (orders) => {
        this.orders = orders;
        this.totalOrders = orders.length;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load orders.';
        this.orders = [];
        this.loading = false;
      }
    });
  }

  fetchPayments() {
    this.http.get<any[]>('/api/Admin/all-payments').subscribe({
      next: (payments) => {
        // Only sum payments that are not Cancelled and (for COD) are Completed
        this.totalPayments = payments
          .filter(p => p.status !== 'Cancelled' && (p.mode !== 'COD' || p.status === 'Completed'))
          .reduce((sum, p) => sum + (p.amount || 0), 0);
      },
      error: () => {
        this.totalPayments = 0;
      }
    });
  }

  updateOrderStatus(order: Order, newStatus: string) {
    if (order.status === newStatus) return;
    this.error = null; // Clear error on new attempt
    this.http.put(`/api/Admin/admin/update-status/${order.orderId}`, JSON.stringify(newStatus), {
      headers: { 'Content-Type': 'application/json' }
    }).subscribe({
      next: (res: any) => {
        if (res && res.error) {
          this.error = res.error;
        } else {
          // Reload the orders to reflect the change
          this.fetchOrders();
          this.error = null;
        }
      },
      error: (err) => {
        // Try to show backend error message if available
        if (err && err.error) {
          this.error = typeof err.error === 'string' ? err.error : (err.error.error || 'Failed to update order status.');
        } else {
          this.error = 'Failed to update order status.';
        }
      }
    });
  }
}
