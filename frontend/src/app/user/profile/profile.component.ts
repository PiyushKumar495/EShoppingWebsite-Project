import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PaymentDialogComponent } from './payment-dialog.component';
import { AuthService } from '../../auth/auth.service';
import { OrderHistoryService, Order } from '../../order/order-history.service';
import { UserPaymentService, UserPayment } from '../user-payment.service';
import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../order/order.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
  standalone: true,
  imports: [MaterialModule, CommonModule, FormsModule]
})
export class ProfileComponent implements OnInit {
  user: any = null;
  orders: Order[] = [];
  payments: UserPayment[] = [];
  totalPayments: number = 0;
  loadingOrders = true;
  loadingPayments = true;
  error = '';
  expandedOrderId: number | null = null;
  cancellingOrderId: number | null = null;
  editingProfile = false;
  editName = '';
  editEmail = '';
  profileSaving = false;
  profileError = '';
  profileSuccess = false;
  // Remove inline payment form state

  constructor(
    private auth: AuthService,
    private orderHistory: OrderHistoryService,
    private userPayment: UserPaymentService,
    private orderService: OrderService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.user = this.auth.getUser();
    if (this.user && this.user.role && this.user.role.toLowerCase() === 'customer') {
      this.editName = this.user.fullName;
      this.editEmail = this.user.email;
      this.orderHistory.getMyOrders().subscribe({
        next: (orders) => {
          this.orders = orders;
          this.loadingOrders = false;
        },
        error: () => {
          this.error = 'Failed to load orders.';
          this.loadingOrders = false;
        }
      });
      this.userPayment.getUserPayments().subscribe({
        next: (res) => {
          this.payments = Array.isArray(res.payments) ? res.payments : [];
          this.totalPayments = res.totalPayments;
          this.loadingPayments = false;
        },
        error: () => {
          this.payments = [];
          this.loadingPayments = false;
        }
      });
    } else {
      this.payments = [];
      this.loadingPayments = false;
    }
  }

  toggleOrderDetails(orderId: number) {
    this.expandedOrderId = this.expandedOrderId === orderId ? null : orderId;
  }

  cancelOrder(orderId: number, status?: string) {
    // Find the order's status if not provided
    const order = this.orders.find(o => o.orderId === orderId);
    const orderStatus = status || order?.status;

    if (orderStatus && orderStatus.toLowerCase() === 'delivered') {
      alert('This order is already delivered and cannot be cancelled.');
      return;
    }
    if (!confirm('Are you sure you want to cancel this order?')) return;
    this.cancellingOrderId = orderId;
    this.orderService.cancelOrder(orderId).subscribe({
      next: () => {
        this.cancellingOrderId = null;
        this.refreshOrders();
      },
      error: () => {
        this.cancellingOrderId = null;
        this.refreshOrders();
      }
    });
  }

  private refreshOrders() {
    this.orderHistory.getMyOrders().subscribe({
      next: (orders) => { this.orders = orders; },
      error: () => {}
    });
  }

  saveProfile() {
    this.profileSaving = true;
    this.profileError = '';
    this.profileSuccess = false;
    const Dto = { name: this.editName, email: this.editEmail };
    this.auth.updateProfile(Dto).subscribe({
      next: () => {
        this.auth.getCurrentUserFromBackend().subscribe({
          next: (user) => {
            // Defensive: handle both fullName and FullName
            this.user.fullName = user.fullName || user.FullName || '';
            this.user.email = user.email || user.Email || '';
            this.user.role = user.role || user.Role || '';
            localStorage.setItem('eshop_user', JSON.stringify(this.user));
            this.profileSaving = false;
            this.profileSuccess = true;
            this.editingProfile = false;
          },
          error: () => {
            this.profileSaving = false;
            this.profileSuccess = true;
            this.editingProfile = false;
          }
        });
      },
      error: (err) => {
        this.profileSaving = false;
        this.profileError = err?.error || 'Failed to update profile.';
      }
    });
  }

  cancelEdit() {
    this.editingProfile = false;
    this.profileError = '';
    this.profileSuccess = false;
    this.editName = this.user.fullName;
    this.editEmail = this.user.email;
  }

  openPayForm(order: any) {
    const dialogRef = this.dialog.open(PaymentDialogComponent, {
      width: '350px',
      data: {
        orderId: order.orderId,
        mode: order.paymentMethod || '',
        amount: order.totalAmount
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result && result.mode && result.amount) {
        this.userPayment.makePayment({
          orderId: order.orderId,
          mode: result.mode,
          amount: result.amount
        }).subscribe({
          next: () => {
            this.ngOnInit();
          },
          error: () => {
            // Optionally show error
          }
        });
      }
    });
  }
}
