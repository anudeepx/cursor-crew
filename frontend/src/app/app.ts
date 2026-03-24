import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { catchError, of } from 'rxjs';
import { AuthService } from './core/services/auth.service';
import { CartService } from './core/services/cart.service';
import { NavbarComponent } from './shared/components/navbar.component';
import { ToastContainerComponent } from './shared/components/toast-container.component';

@Component({
  selector: 'app-root',
  imports: [NavbarComponent, ToastContainerComponent, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('frontend');

  constructor(
    private readonly authService: AuthService,
    private readonly cartService: CartService
  ) { }

  ngOnInit(): void {
    if (!this.authService.isLoggedIn) {
      return;
    }

    this.authService.me().pipe(catchError(() => of(null))).subscribe();
    this.cartService.refreshCart().pipe(catchError(() => of({ items: [], totalAmount: 0 }))).subscribe();
  }
}
