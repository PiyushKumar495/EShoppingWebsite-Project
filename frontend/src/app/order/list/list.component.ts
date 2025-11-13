import { Component, OnInit } from '@angular/core';
import { OrderHistoryService, Order } from '../order-history.service';
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
  orders: Order[] = [];
  loading = true;
  error = '';
  isAdmin: boolean = false;

  constructor(private orderHistoryService: OrderHistoryService) {}

  ngOnInit() {
    this.orderHistoryService.getMyOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load order history.';
        this.loading = false;
      }
    });
    // Check if user is admin (simple localStorage or service check, adjust as needed)
    this.isAdmin = localStorage.getItem('eshop_role') === 'Admin';
  }

  canChangeStatus(order: Order): boolean {
    // Only allow status change if not delivered or cancelled
    return this.isAdmin && order.status !== 'Delivered' && order.status !== 'Cancelled';
  }

  getAvailableStatuses(order: Order): string[] {
    // If shipped, cannot go back to pending
    const all = ['Pending', 'Shipped', 'Delivered', 'Cancelled'];
    if (order.status === 'Shipped') return ['Shipped', 'Delivered', 'Cancelled'];
    return all;
  }

  onStatusChange(order: Order, newStatus: string) {
    if (order.status === newStatus) return;
    this.orderHistoryService.updateOrderStatus(order.orderId, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
      },
      error: err => {
        alert(err.error?.message || 'Failed to update order status');
      }
    });
  }
}
