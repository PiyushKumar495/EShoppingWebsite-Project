import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { MaterialModule } from '../../material.module';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-payment-dialog',
  template: `
    <h2 mat-dialog-title><mat-icon class="section-icon">payments</mat-icon> Pay for Order #{{ data.orderId }}</h2>
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-form-field appearance="fill" style="width: 100%;">
        <mat-label>Mode</mat-label>
        <mat-select formControlName="mode" required>
          <mat-option value="UPI">UPI</mat-option>
          <mat-option value="COD">COD</mat-option>
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="fill" style="width: 100%;">
        <mat-label>Amount</mat-label>
        <input matInput formControlName="amount" required type="number" min="1" />
      </mat-form-field>
      <div style="display: flex; gap: 12px; margin-top: 18px;">
        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || loading">Pay</button>
        <button mat-button type="button" (click)="onCancel()" [disabled]="loading">Cancel</button>
      </div>
      <mat-progress-bar *ngIf="loading" mode="indeterminate"></mat-progress-bar>
      <div *ngIf="error" class="error-state">{{ error }}</div>
      <div *ngIf="success" class="success-state">Payment successful!</div>
    </form>
  `
  ,
  standalone: true,
  imports: [MaterialModule, CommonModule, ReactiveFormsModule]
})
export class PaymentDialogComponent {
  form: FormGroup;
  loading = false;
  error = '';
  success = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<PaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { orderId: number, mode: string, amount: number },
  ) {
    this.form = this.fb.group({
      mode: [data.mode, Validators.required],
      amount: [data.amount, [Validators.required, Validators.min(1)]]
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.success = false;
    this.dialogRef.close({
      mode: this.form.value.mode,
      amount: this.form.value.amount
    });
  }

  onCancel() {
    this.dialogRef.close(null);
  }
}
