import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../shared/models/order';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <section class="page">
      <h1>User Dashboard</h1>
      <p class="muted">View your profile, order activity, and quick actions.</p>

      <div class="grid dashboard-grid" *ngIf="authService.currentUser as user">
        <article class="card">
          <h3>Profile</h3>
          <p><strong>Name:</strong> {{ user.name }}</p>
          <p><strong>Email:</strong> {{ user.email }}</p>
          <p><strong>Role:</strong> {{ user.role }}</p>
        </article>

        <article class="card">
          <h3>Order Summary</h3>
          <p><strong>Total orders:</strong> {{ orders.length }}</p>
          <p><strong>Pending:</strong> {{ pendingOrders }}</p>
          <p><strong>Total spent:</strong> {{ totalSpent | currency }}</p>
        </article>

        <article class="card">
          <h3>Quick Actions</h3>
          <div class="dashboard-actions">
            <a class="primary action-link" routerLink="/products">Continue shopping</a>
            <a class="secondary action-link" routerLink="/orders">Track orders</a>
            <a class="secondary action-link" routerLink="/cart">Go to cart</a>
          </div>
        </article>
      </div>

      <p class="message" *ngIf="errorMessage">{{ errorMessage }}</p>
      <p class="muted" *ngIf="loading">Loading dashboard...</p>
    </section>
  `
})
export class DashboardComponent implements OnInit {
    orders: Order[] = [];
    loading = false;
    errorMessage = '';

    constructor(
        readonly authService: AuthService,
        private readonly orderService: OrderService
    ) { }

    get totalSpent(): number {
        return this.orders.reduce((sum, order) => sum + order.totalAmount, 0);
    }

    get pendingOrders(): number {
        return this.orders.filter((order) => order.status.toLowerCase() !== 'delivered').length;
    }

    ngOnInit(): void {
        if (!this.authService.currentUser) {
            this.errorMessage = 'Please login to view your dashboard.';
            return;
        }

        this.loading = true;
        this.orderService.getOrders().subscribe({
            next: (orders) => {
                this.orders = orders;
                this.loading = false;
            },
            error: () => {
                this.errorMessage = 'Unable to load dashboard data.';
                this.loading = false;
            }
        });
    }
}
