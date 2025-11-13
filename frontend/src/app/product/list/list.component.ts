import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ProductService, Product } from '../product.service';
import { CartService } from '../../cart/cart.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrl: './list.component.css',
  standalone: true,
  imports: [MaterialModule, CommonModule, FormsModule]
})
export class ListComponent implements OnInit {
  products: Product[] = [];
  loading = true;
  error = '';
  categorySearch: string = '';
  productNameSearch: string = '';
  searching = false;

  constructor(
    private productService: ProductService,
    private cartService: CartService,
    private snackBar: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute // Inject ActivatedRoute
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const search = params['search'];
      if (search && search.trim()) {
        this.handleUnifiedSearch(search.trim());
      } else {
        this.loadAllProducts();
      }
    });
  }

  handleUnifiedSearch(search: string) {
    this.loading = true;
    this.error = '';
    // Try category search first
    this.productService.getByCategoryName(search).subscribe({
      next: (category: any) => {
        if (category && category.products && Array.isArray(category.products) && category.products.length > 0) {
          this.products = category.products.map((p: any) => ({ ...p, selectedQuantity: 1 }));
          this.loading = false;
        } else {
          // If no category products, try product name
          this.productService.getByName(search).subscribe({
            next: (product) => {
              this.products = product ? [{ ...product, selectedQuantity: 1 }] : [];
              this.loading = false;
              if (!product) this.error = 'No products found.';
            },
            error: () => {
              this.products = [];
              this.loading = false;
              this.error = 'No products found.';
            }
          });
        }
      },
      error: () => {
        // If category search fails, try product name
        this.productService.getByName(search).subscribe({
          next: (product) => {
            this.products = product ? [{ ...product, selectedQuantity: 1 }] : [];
            this.loading = false;
            if (!product) this.error = 'No products found.';
          },
          error: () => {
            this.products = [];
            this.loading = false;
            this.error = 'No products found.';
          }
        });
      }
    });
  }

  searchByCategory() {
    this.loading = true;
    this.error = '';
    if (!this.categorySearch) {
      this.productService.getAll().subscribe({
        next: (data) => {
          this.products = data.map(p => ({ ...p, selectedQuantity: 1 }));
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Failed to load products.';
          this.loading = false;
        }
      });
      return;
    }
    this.productService.getByCategoryName(this.categorySearch).subscribe({
      next: (category: any) => {
        if (category && category.products && Array.isArray(category.products)) {
          this.products = category.products.map((p: any) => ({ ...p, selectedQuantity: 1 }));
        } else {
          this.products = [];
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Category not found or failed to load.';
        this.products = [];
        this.loading = false;
      }
    });
  }

  searchByProductName() {
    if (!this.productNameSearch) return;
    this.loading = true;
    this.error = '';
    this.productService.getByName(this.productNameSearch).subscribe({
      next: (product) => {
        this.products = product ? [{ ...product, selectedQuantity: 1 }] : [];
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Product not found.';
        this.products = [];
        this.loading = false;
      }
    });
  }

  loadAllProducts() {
    this.loading = true;
    this.productService.getAll().subscribe({
      next: (data) => {
        this.products = data.map(p => ({ ...p, selectedQuantity: 1 }));
        this.loading = false;
        this.error = '';
      },
      error: (err) => {
        this.error = 'Failed to load products.';
        this.loading = false;
      }
    });
  }

  goToDetail(product: Product) {
    this.router.navigate(['/product/detail', product.productId]);
  }

  adDtoCart(product: Product, quantity: number = 1) {
    const snackBarConfig = { duration: 2000, horizontalPosition: 'center' as const, verticalPosition: 'top' as const };
    if (quantity > product.stockQuantity) {
      this.snackBar.open('Cannot add more than available stock!', 'Close', snackBarConfig);
      return;
    }
    console.log('Adding to cart:', { productId: product.productId, quantity }); // Updated to use productId
    if (!localStorage.getItem('eshop_token')) {
      this.cartService.adDtoCartGuest({ productId: product.productId, name: product.name, price: product.price, stockQuantity: product.stockQuantity }, quantity).subscribe({
        next: () => this.snackBar.open('Added to cart!', 'Close', snackBarConfig),
        error: (err) => this.snackBar.open(err.message || 'Failed to add to cart', 'Close', snackBarConfig)
      });
    } else {
      this.cartService.adDtoCart(product.name, quantity).subscribe({
        next: () => this.snackBar.open('Added to cart!', 'Close', snackBarConfig),
        error: (err) => {
          console.error('Error adding to cart:', err); // Debugging log
          if (err.error && err.error.errors) {
            console.error('Validation errors:', err.error.errors); // Log validation errors
          }
          this.snackBar.open('Failed to add to cart', 'Close', snackBarConfig);
        }
      });
    }
  }

  decrementQuantity(product: Product) {
    product.selectedQuantity = Math.max(1, (product.selectedQuantity || 1) - 1);
  }

  incrementQuantity(product: Product) {
    product.selectedQuantity = Math.min(product.stockQuantity, (product.selectedQuantity || 1) + 1);
  }
}
