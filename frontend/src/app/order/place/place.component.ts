import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OrderService } from '../order.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { UserPaymentService } from '../../user/user-payment.service';

@Component({
  selector: 'app-place',
  templateUrl: './place.component.html',
  styleUrl: './place.component.css',
  standalone: true,
  imports: [MaterialModule, CommonModule, ReactiveFormsModule]
})
export class PlaceComponent {
  orderForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private snackBar: MatSnackBar,
    private router: Router,
    private userPayment: UserPaymentService
  ) {
    this.orderForm = this.fb.group({
      shippingAddress: ['', Validators.required],
      paymentMethod: ['COD', Validators.required]
    });
  }

  onSubmit() {
    if (this.orderForm.invalid) return;
    this.loading = true;
    const orderData = this.orderForm.value;
    this.orderService.placeOrder(orderData).subscribe({
      next: (order: any) => {
        // Clear guest cart after successful order (for guest users)
        localStorage.removeItem('eshop_guest_cart');
        if (order && order.paymentMethod === 'UPI') {
          this.userPayment.makePayment({
            orderId: order.orderId || order.OrderId,
            mode: 'UPI',
            amount: order.totalAmount || order.TotalAmount
          }).subscribe({
            next: () => {
              this.snackBar.open('Payment successful!', 'Close', { duration: 2000 });
              this.loading = false;
              this.router.navigate(['/']);
            },
            error: () => {
              this.snackBar.open('Order placed, but payment failed!', 'Close', { duration: 3000 });
              this.loading = false;
              this.router.navigate(['/']);
            }
          });
        } else {
          this.snackBar.open('Order placed successfully!', 'Close', { duration: 2000 });
          this.loading = false;
          this.router.navigate(['/']);
        }
      },
      error: err => {
        this.snackBar.open(err.error || 'Order failed', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}
