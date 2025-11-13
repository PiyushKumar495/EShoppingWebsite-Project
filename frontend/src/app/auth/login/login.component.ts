import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  standalone: true,
  imports: [MaterialModule, CommonModule, ReactiveFormsModule]
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) return;
    this.loading = true;
    this.auth.login(this.loginForm.value).subscribe({
      next: res => {
        this.auth.setSession(res);
        // Merge guest cart if needed
        this.auth.mergeGuestCartIfNeeded().subscribe({
          next: () => {
            localStorage.removeItem('eshop_guest_cart');
            // Dispatch a custom event so cart reloads from server
            window.dispatchEvent(new Event('eshop_login'));
            this.snackBar.open('Login successful!', 'Close', { duration: 2000 });
            this.loading = false;
            // Redirect based on role or cart redirect
            if (localStorage.getItem('eshop_cart_redirect')) {
              localStorage.removeItem('eshop_cart_redirect');
              this.router.navigate(['/order/place']);
            } else if (res.user.role && res.user.role.toLowerCase() === 'admin') {
              this.router.navigate(['/admin-dashboard']);
            } else {
              this.router.navigate(['/product/list']);
            }
          },
          error: () => {
            localStorage.removeItem('eshop_guest_cart');
            window.dispatchEvent(new Event('eshop_login'));
            this.snackBar.open('Login successful!', 'Close', { duration: 2000 });
            this.loading = false;
            if (localStorage.getItem('eshop_cart_redirect')) {
              localStorage.removeItem('eshop_cart_redirect');
              this.router.navigate(['/order/place']);
            } else if (res.user.role && res.user.role.toLowerCase() === 'admin') {
              this.router.navigate(['/admin-dashboard']);
            } else {
              this.router.navigate(['/product/list']);
            }
          }
        });
      },
      error: err => {
        this.snackBar.open(err.error || 'Login failed', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}
