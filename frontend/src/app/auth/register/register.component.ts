import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  standalone: true,
  imports: [MaterialModule, CommonModule, ReactiveFormsModule]
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
       // Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*()_+\-=[\]{};\':\"\\|,.<>\/?]).{8,}$')
      ]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordsMatch });
  }

  passwordsMatch(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    // Only show error if both fields are touched and not empty
    if (!password || !confirmPassword) return null;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit() {
    if (this.registerForm.invalid) return;
    this.loading = true;
    this.auth.register(this.registerForm.value).subscribe({
      next: (res) => {
        this.loading = false;
        this.registerForm.reset();
        this.router.navigate(['/login']);
      },
      error: err => {
        let msg = 'Registration failed';
        if (err.status === 200) {
          msg = 'Registration successful! Please login.';
          this.router.navigate(['/login']);
        } else if (err.error) {
          if (typeof err.error === 'string') msg = err.error;
          else if (err.error.message) msg = err.error.message;
        }
        this.snackBar.open(msg, 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}
