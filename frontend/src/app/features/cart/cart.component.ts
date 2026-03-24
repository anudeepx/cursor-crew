import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { NotificationService } from '../../core/services/notification.service';
import { OrderService } from '../../core/services/order.service';
import { CartItem } from '../../shared/models/cart';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="page">
      <h1>Cart</h1>
      <p class="muted">Review your items before checkout.</p>

      <div *ngIf="!(authService.user$ | async)">
        <p class="message">Please <a routerLink="/login">login</a> to place an order.</p>
      </div>

      <ng-container *ngIf="items$ | async as items">
        <p *ngIf="!items.length" class="muted">Your cart is empty.</p>

        <div *ngIf="items.length">
          <div class="cart-row" *ngFor="let item of items">
            <div>
                <strong>{{ item.productName }}</strong>
              <div class="muted">Product #{{ item.productId }}</div>
            </div>
              <div>{{ item.unitPrice | currency }}</div>
            <input
              type="number"
              min="1"
              [value]="item.quantity"
              (change)="updateQuantity(item, $any($event.target).value)"
            />
            <button class="secondary" type="button" (click)="remove(item.productId)">
              Remove
            </button>
          </div>

          <div class="total-row">
            <span>Total</span>
            <span>{{ total | currency }}</span>
          </div>

          <div class="coupon-row">
            <input
              type="text"
              placeholder="Promo code (SAVE10, WELCOME15, REORDER5)"
              [(ngModel)]="couponCode"
            />
            <button class="secondary" type="button" (click)="applyCoupon()">Apply</button>
          </div>

          <p class="muted" *ngIf="activeCouponCode">
            Coupon {{ activeCouponCode }} applied ({{ discountPercentage }}% off): -{{ discountAmount | currency }}
          </p>

          <div class="total-row" *ngIf="activeCouponCode">
            <span>Estimated total after coupon</span>
            <span>{{ discountedTotal | currency }}</span>
          </div>

          <div class="form-control" style="max-width: 340px; margin: 14px 0 8px;">
            <label for="paymentMethod">Payment option</label>
            <select id="paymentMethod" [(ngModel)]="selectedPaymentMethod" disabled>
              <option [ngValue]="hardcodedPaymentMethod">{{ hardcodedPaymentMethod }}</option>
            </select>
          </div>
          <p class="muted">Temporary checkout mode: only {{ hardcodedPaymentMethod }} is available.</p>

          <button
            class="primary"
            type="button"
            [disabled]="isPlacingOrder || !items.length"
            (click)="checkout(items)"
          >
            {{ isPlacingOrder ? 'Placing order...' : 'Checkout' }}
          </button>

          <p class="message" *ngIf="errorMessage">{{ errorMessage }}</p>
          <p class="message success" *ngIf="successMessage">{{ successMessage }}</p>
        </div>
      </ng-container>
    </section>
  `
})
export class CartComponent implements OnInit {
  readonly items$: Observable<CartItem[]>;
  readonly hardcodedPaymentMethod = 'Cash on Delivery';
  readonly promoCoupons: Record<string, number> = {
    SAVE10: 10,
    WELCOME15: 15,
    REORDER5: 5
  };
  selectedPaymentMethod = this.hardcodedPaymentMethod;
  couponCode = '';
  activeCouponCode = '';
  errorMessage = '';
  successMessage = '';
  isPlacingOrder = false;

  constructor(
    readonly authService: AuthService,
    private readonly cartService: CartService,
    private readonly orderService: OrderService,
    private readonly notificationService: NotificationService,
    private readonly router: Router
  ) {
    this.items$ = this.cartService.items$;
  }

  ngOnInit(): void {
    if (!this.authService.isLoggedIn) {
      return;
    }

    this.cartService.refreshCart().subscribe({
      error: () => {
        this.errorMessage = 'Unable to load cart.';
      }
    });
  }

  get total(): number {
    return this.cartService.getTotal();
  }

  get discountPercentage(): number {
    return this.activeCouponCode ? this.promoCoupons[this.activeCouponCode] : 0;
  }

  get discountAmount(): number {
    return this.total * (this.discountPercentage / 100);
  }

  get discountedTotal(): number {
    return Math.max(this.total - this.discountAmount, 0);
  }

  applyCoupon(): void {
    const normalized = this.couponCode.trim().toUpperCase();
    if (!normalized) {
      this.activeCouponCode = '';
      return;
    }

    if (!this.promoCoupons[normalized]) {
      this.activeCouponCode = '';
      this.errorMessage = 'Invalid coupon code.';
      this.notificationService.showWarning('Coupon code is invalid.');
      return;
    }

    this.activeCouponCode = normalized;
    this.errorMessage = '';
    this.notificationService.showSuccess(`Coupon ${normalized} applied.`);
  }

  updateQuantity(item: CartItem, value: string): void {
    const quantity = Number(value);
    if (Number.isNaN(quantity) || quantity < 1) {
      return;
    }

    this.cartService.updateQuantity(item.productId, quantity).subscribe({
      next: () => {
        this.notificationService.showInfo('Cart quantity updated.');
      },
      error: () => {
        this.errorMessage = 'Unable to update quantity.';
      }
    });
  }

  remove(productId: number): void {
    this.cartService.removeFromCart(productId).subscribe({
      next: () => {
        this.notificationService.showInfo('Item removed from cart.');
      },
      error: () => {
        this.errorMessage = 'Unable to remove item.';
      }
    });
  }

  checkout(items: CartItem[]): void {
    if (!this.authService.isLoggedIn) {
      this.router.navigate(['/login']);
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.isPlacingOrder = true;

    // Temporary: payment is hardcoded until payment gateway integration is enabled.
    this.selectedPaymentMethod = this.hardcodedPaymentMethod;

    const payload = items.map((item) => ({
      productId: item.productId,
      quantity: item.quantity
    }));

    this.orderService.placeOrder(payload).subscribe({
      next: (response) => {
        this.isPlacingOrder = false;
        if (!response.success) {
          this.errorMessage = response.message || 'Unable to place order.';
          return;
        }
        this.successMessage = response.message || 'Order placed successfully.';
        this.cartService.clearCart().subscribe();
        this.notificationService.showSuccess('Order placed successfully.');
        this.router.navigate(['/orders']);
      },
      error: () => {
        this.isPlacingOrder = false;
        this.errorMessage = 'Unable to place order.';
      }
    });
  }
}
