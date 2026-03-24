import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { NotificationService } from '../../core/services/notification.service';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../shared/models/order';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="page">
      <h1>Orders</h1>
      <p class="muted">Track your past orders.</p>

      <div class="filters-row" style="margin-top: 10px;">
        <div class="form-control">
          <label for="statusFilter">Status</label>
          <select id="statusFilter" [(ngModel)]="statusFilter">
            <option value="all">All</option>
            <option value="pending">Pending</option>
            <option value="confirmed">Confirmed</option>
            <option value="preparing">Preparing</option>
            <option value="outfordelivery">Out for delivery</option>
            <option value="delivered">Delivered</option>
          </select>
        </div>

        <div class="form-control">
          <label for="orderSearch">Order #</label>
          <input id="orderSearch" type="text" [(ngModel)]="searchOrderId" placeholder="Search by order id" />
        </div>
      </div>

      <p class="message" *ngIf="errorMessage">{{ errorMessage }}</p>

      <ng-container *ngIf="!loading">
        <p class="muted" *ngIf="!filteredOrders.length">
          No orders yet. <a routerLink="/products">Start shopping</a>.
        </p>

        <div class="card" *ngFor="let order of filteredOrders">
          <h3>Order #{{ order.id }}</h3>
          <p class="muted">Placed: {{ order.createdAt | date: 'medium' }}</p>
          <p>Status: <strong>{{ order.status }}</strong></p>
          <div class="muted">Total: {{ order.totalAmount | currency }}</div>

          <button
            class="secondary"
            type="button"
            [disabled]="reorderingOrderId === order.id"
            (click)="reorder(order)"
          >
            {{ reorderingOrderId === order.id ? 'Reordering...' : 'Quick reorder' }}
          </button>

          <div style="margin-top: 12px;">
            <div class="cart-row" *ngFor="let item of order.items">
              <div>{{ item.productName }}</div>
              <div>{{ item.price | currency }}</div>
              <div>Qty: {{ item.quantity }}</div>
              <div>{{ item.price * item.quantity | currency }}</div>
            </div>
          </div>
        </div>
      </ng-container>

      <p class="muted" *ngIf="loading">Loading orders...</p>
    </section>
  `
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  statusFilter = 'all';
  searchOrderId = '';
  reorderingOrderId: number | null = null;
  loading = false;
  errorMessage = '';

  constructor(
    private readonly authService: AuthService,
    private readonly orderService: OrderService,
    private readonly cartService: CartService,
    private readonly notificationService: NotificationService,
    private readonly router: Router
  ) { }

  get filteredOrders(): Order[] {
    return this.orders.filter((order) => {
      const statusMatch = this.statusFilter === 'all' || order.status.toLowerCase() === this.statusFilter;
      const orderIdMatch = !this.searchOrderId || String(order.id).includes(this.searchOrderId.trim());
      return statusMatch && orderIdMatch;
    });
  }

  ngOnInit(): void {
    const user = this.authService.currentUser;
    if (!user) {
      this.errorMessage = 'Please login to view your orders.';
      return;
    }

    this.loading = true;
    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load orders.';
        this.loading = false;
      }
    });
  }

  reorder(order: Order): void {
    if (!order.items.length) {
      return;
    }

    this.reorderingOrderId = order.id;
    const requests = order.items.map((item) =>
      this.cartService.addToCart(item.productId, item.quantity, {
        productName: item.productName,
        unitPrice: item.price
      })
    );

    forkJoin(requests).subscribe({
      next: () => {
        this.notificationService.showSuccess(`Order #${order.id} added to cart.`);
        this.reorderingOrderId = null;
        this.router.navigate(['/cart']);
      },
      error: () => {
        this.notificationService.showError('Unable to reorder items right now.');
        this.reorderingOrderId = null;
      }
    });
  }
}
