import { AsyncPipe, CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { map, Observable } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, AsyncPipe],
  template: `
    <nav class="navbar">
      <div class="navbar__brand">
        <a routerLink="/products" class="brand">Retail Ordering</a>
      </div>
      <div class="navbar__links">
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a routerLink="/products" routerLinkActive="active">Products</a>
        <a routerLink="/cart" routerLinkActive="active">
          Cart ({{ itemCount$ | async }})
        </a>
        <a routerLink="/orders" routerLinkActive="active">Orders</a>
      </div>
      <div class="navbar__auth">
        <ng-container *ngIf="authService.user$ | async as user; else loggedOut">
          <span class="navbar__user">Hi, {{ user.name }}</span>
          <button class="link-button" type="button" (click)="logout()">Logout</button>
        </ng-container>
        <ng-template #loggedOut>
          <a routerLink="/login">Login</a>
          <a routerLink="/register">Register</a>
        </ng-template>
      </div>
    </nav>
  `
})
export class NavbarComponent {
  readonly itemCount$: Observable<number>;

  constructor(
    readonly authService: AuthService,
    private readonly cartService: CartService,
    private readonly router: Router
  ) {
    this.itemCount$ = this.cartService.items$.pipe(
      map((items) => items.reduce((total, item) => total + item.quantity, 0))
    );
  }

  logout(): void {
    this.authService.logoutRequest().subscribe({
      next: () => {
        this.authService.logout();
        this.cartService.clearLocalCart();
        this.router.navigate(['/login']);
      },
      error: () => {
        this.authService.logout();
        this.cartService.clearLocalCart();
        this.router.navigate(['/login']);
      }
    });
  }
}
