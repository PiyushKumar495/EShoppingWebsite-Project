import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService, Product } from '../product.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CartService } from '../../cart/cart.service';
import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-detail',
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.css',
  standalone: true,
  imports: [MaterialModule, CommonModule, FormsModule]
})
export class DetailComponent implements OnInit {
  product: Product | null = null;
  loading = true;
  error = '';
  quantity = 1;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private cartService: CartService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.productService.getAll().subscribe({
        next: (products) => {
          this.product = products.find(p => p.productId === +id) || null;
          if (!this.product) this.error = 'Product not found.';
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load product.';
          this.loading = false;
        }
      });
    } else {
      this.error = 'Invalid product ID.';
      this.loading = false;
    }
  }

  adDtoCart() {
    if (!this.product) return;
    // Prevent adding more than available stock from frontend
    if (this.quantity > this.product.stockQuantity) {
      this.snackBar.open('Cannot add more than available stock!', 'Close', { duration: 2000 });
      return;
    }
    // Use correct method for guest/logged-in
    if ((this.cartService as any).isLoggedIn && typeof (this.cartService as any).isLoggedIn === 'function' && (this.cartService as any).isLoggedIn()) {
      this.cartService.adDtoCart(this.product.name, this.quantity).subscribe({
        next: () => this.snackBar.open('Added to cart!', 'Close', { duration: 1500 }),
        error: (err) => this.snackBar.open(err.error?.message || 'Failed to add to cart', 'Close', { duration: 2000 })
      });
    } else {
      this.cartService.adDtoCartGuest(this.product, this.quantity).subscribe({
        next: () => this.snackBar.open('Added to cart!', 'Close', { duration: 1500 }),
        error: (err) => this.snackBar.open(err.message || 'Failed to add to cart', 'Close', { duration: 2000 })
      });
    }
  }
}
