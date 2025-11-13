import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CartItem {
  cartItemId: number;
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  totalPrice: number;
}

export interface CartResponse {
  cartId: number;
  items: CartItem[];
  granDtotal: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  // Call this after successful merge to clear guest cart
  clearGuestCart() {
    localStorage.removeItem(this.localCartKey);
  }
  private apiUrl = '/api/cart'; // Use proxy for backend
  private localCartKey = 'eshop_guest_cart';

  constructor(private http: HttpClient) {}

  private isLoggedIn(): boolean {
    return !!localStorage.getItem('eshop_token');
  }

  getCart(): Observable<CartResponse> {
    // Always fetch from server if logged in
    if (this.isLoggedIn()) {
      return this.http.get<CartResponse>(this.apiUrl);
    }
    // Only fallback to localStorage if not logged in
    const cart = this.getLocalCart();
    return new Observable<CartResponse>((observer) => {
      observer.next(cart);
      observer.complete();
    });
  }

  adDtoCart(productName: string, quantity: number): Observable<any> {
    if (this.isLoggedIn()) {
      return this.http.post(`${this.apiUrl}/add`, { productName, quantity });
    } else {
      const cart = this.getLocalCart();
      // Check if item exists
      const idx = cart.items.findIndex(i => i.productName === productName);
      if (idx > -1) {
        cart.items[idx].quantity += quantity;
        cart.items[idx].totalPrice = cart.items[idx].quantity * cart.items[idx].price;
      } else {
        // For guest cart, you may want to fetch product details from backend, but for now, use placeholder
        cart.items.push({
          cartItemId: Date.now(),
          productId: 0, // Placeholder ID
          productName, // Use productName
          price: 0, // Placeholder price
          quantity,
          totalPrice: 0
        });
      }
      cart.granDtotal = cart.items.reduce((sum, i) => sum + i.totalPrice, 0);
      this.saveLocalCart(cart);
      return new Observable(observer => {
        observer.next({ message: 'Added to cart' });
        observer.complete();
      });
    }
  }

  // Merge guest cart with user cart after login
  mergeGuestCartWithUser(guestItems: any[]): Observable<any> {
    return new Observable(observer => {
      this.http.post(`${this.apiUrl}/merge`, { items: guestItems }).subscribe({
        next: (res) => {
          this.clearGuestCart();
          observer.next(res);
          observer.complete();
        },
        error: (err) => {
          observer.error(err);
          observer.complete();
        }
      });
    });
  }

  // Add product to guest cart with full product info and quantity
  adDtoCartGuest(product: { productId: number; name: string; price: number; stockQuantity: number }, quantity: number = 1): Observable<any> {
    const cart = this.getLocalCart();
    const idx = cart.items.findIndex(i => i.productId === product.productId);
    let newQuantity = quantity;
    if (idx > -1) {
      newQuantity = cart.items[idx].quantity + quantity;
      if (newQuantity > product.stockQuantity) {
        return new Observable(observer => {
          observer.error({ message: 'Cannot add more than available stock!' });
          observer.complete();
        });
      }
      cart.items[idx].quantity = newQuantity;
      cart.items[idx].totalPrice = cart.items[idx].quantity * product.price;
    } else {
      if (quantity > product.stockQuantity) {
        return new Observable(observer => {
          observer.error({ message: 'Cannot add more than available stock!' });
          observer.complete();
        });
      }
      cart.items.push({
        cartItemId: Date.now(),
        productId: product.productId,
        productName: product.name,
        price: product.price,
        quantity: quantity,
        totalPrice: product.price * quantity
      });
    }
    cart.granDtotal = cart.items.reduce((sum, i) => sum + i.totalPrice, 0);
    this.saveLocalCart(cart);
    return new Observable(observer => {
      observer.next({ message: 'Added to cart' });
      observer.complete();
    });
  }

  removeFromCart(cartItemId: number): Observable<any> {
    if (this.isLoggedIn()) {
      return this.http.delete(`${this.apiUrl}/item/${cartItemId}`);
    } else {
      const cart = this.getLocalCart();
      cart.items = cart.items.filter(i => i.cartItemId !== cartItemId);
      cart.granDtotal = cart.items.reduce((sum, i) => sum + i.totalPrice, 0);
      this.saveLocalCart(cart);
      return new Observable(observer => {
        observer.next({ message: 'Removed from cart' });
        observer.complete();
      });
    }
  }

  clearCart(): Observable<any> {
    if (this.isLoggedIn()) {
      return this.http.delete(`${this.apiUrl}/clear`);
    } else {
      this.saveLocalCart({ cartId: 0, items: [], granDtotal: 0 });
      return new Observable(observer => {
        observer.next({ message: 'Cart cleared' });
        observer.complete();
      });
    }
  }

  // Local cart helpers
  private getLocalCart(): CartResponse {
    const cart = localStorage.getItem(this.localCartKey);
    return cart ? JSON.parse(cart) : { cartId: 0, items: [], granDtotal: 0 };
  }

  private saveLocalCart(cart: CartResponse) {
    localStorage.setItem(this.localCartKey, JSON.stringify(cart));
  }
}
