import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  token: string;
  user: {
    userId: number;
    fullName: string;
    email: string;
    role: string;
  };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5208/api/auth';

  constructor(private http: HttpClient) {}

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap((authResponse) => {
        this.setSession(authResponse);
        this.mergeGuestCartIfNeeded().subscribe({
          next: () => console.log('Guest cart merged successfully.'),
          error: (err) => console.error('Failed to merge guest cart:', err)
        });
      })
    );
  }

  register(data: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  setSession(auth: AuthResponse) {
    localStorage.setItem('eshop_token', auth.token);
    localStorage.setItem('eshop_user', JSON.stringify(auth.user));
  }

  logout() {
    localStorage.removeItem('eshop_token');
    localStorage.removeItem('eshop_user');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('eshop_token');
  }

  getUser() {
    const user = localStorage.getItem('eshop_user');
    return user ? JSON.parse(user) : null;
  }

  isAdmin(): boolean {
    const user = this.getUser();
    return user && user.role && user.role.toLowerCase() === 'admin';
  }

  updateProfile(data: { name: string; email: string }) {
    return this.http.put('/api/User/profile', data);
  }

  /**
   * Fetches the current user from the backend and returns an observable.
   */
  getCurrentUserFromBackend() {
    return this.http.get<any>('/api/User/profile');
  }

  /**
   * Call this after successful login
   * Merges the guest cart with the backend cart using the /api/cart/merge endpoint.
   */
  mergeGuestCartIfNeeded(): Observable<any> {
    const guestCart = localStorage.getItem('eshop_guest_cart');
    if (!guestCart) {
      return new Observable(observer => {
        observer.next({ message: 'No guest cart to merge.' });
        observer.complete();
      });
    }
    const guestCartObj = JSON.parse(guestCart);
    // Group guest cart items by productId and sum quantities
    const grouped: Record<number, number> = {};
    (guestCartObj.items || []).forEach((item: any) => {
      const pid = Number(item.productId);
      if (!grouped[pid]) {
        grouped[pid] = 0;
      }
      grouped[pid] += item.quantity;
    });
    const items = Object.entries(grouped).map(([pid, quantity]) => ({
      productId: Number(pid),
      quantity: quantity as number
    }));
    return this.http.post('/api/cart/merge', { items });
  }
}
