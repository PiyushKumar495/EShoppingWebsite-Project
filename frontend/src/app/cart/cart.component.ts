import { Component, OnInit } from '@angular/core';
import { CartService, CartResponse, CartItem } from './cart.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MaterialModule } from '../material.module';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
  standalone: true,
  imports: [MaterialModule, CommonModule]
})
export class CartComponent implements OnInit {
  cart: CartResponse | null = null;
  loading = true;
  error = '';

  constructor(
    private cartService: CartService,
    private snackBar: MatSnackBar,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadCart();
    // Listen for login event to reload cart from server
    window.addEventListener('eshop_login', () => {
      this.loadCart();
    });
  }

  loadCart() {
    this.loading = true;
    this.cartService.getCart().subscribe({
      next: (cart) => {
        this.cart = cart;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load cart.';
        this.loading = false;
      }
    });
  }

  removeItem(item: CartItem) {
    this.cartService.removeFromCart(item.cartItemId).subscribe({
      next: () => {
        this.snackBar.open('Item removed from cart', 'Close', { duration: 1500 });
        this.loadCart();
      },
      error: () => this.snackBar.open('Failed to remove item', 'Close', { duration: 2000 })
    });
  }

  clearCart() {
    this.cartService.clearCart().subscribe({
      next: () => {
        this.snackBar.open('Cart cleared', 'Close', { duration: 1500 });
        this.loadCart();
      },
      error: () => this.snackBar.open('Failed to clear cart', 'Close', { duration: 2000 })
    });
  }

  onCheckout() {
    if (!this.authService.isLoggedIn()) {
      // Set a redirect flag and preserve cart in localStorage (already handled by cart service)
      localStorage.setItem('eshop_cart_redirect', '1');
      this.snackBar.open('Please login to place your order.', 'Close', { duration: 2000, horizontalPosition: 'center', verticalPosition: 'top' });
      this.router.navigate(['/login']);
    } else {
      this.router.navigate(['/order/place']);
    }
  }
}
