import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { NotificationService } from '../../core/services/notification.service';
import { Brand, ProductService } from '../../core/services/product.service';
import { Category, Product } from '../../shared/models/product';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page">
      <h1>Products</h1>
      <p class="muted">Browse pizzas, drinks, and bread with live stock visibility.</p>

      <div class="form-control" style="max-width: 420px; margin-bottom: 14px;">
        <label for="search">Search</label>
        <input
          id="search"
          type="text"
          placeholder="Search by product, category, or brand"
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearchChange($event)"
        />
      </div>

      <div class="filters-row">
        <div class="form-control">
          <label for="category">Category</label>
          <select id="category" [(ngModel)]="selectedCategoryId" (ngModelChange)="onFilterChange()">
            <option [ngValue]="0">All categories</option>
            <option *ngFor="let category of categories" [ngValue]="category.id">
              {{ category.name }}
            </option>
          </select>
        </div>

        <div class="form-control">
          <label for="brand">Brand</label>
          <select id="brand" [(ngModel)]="selectedBrandId" (ngModelChange)="onFilterChange()">
            <option [ngValue]="0">All brands</option>
            <option *ngFor="let brand of brands" [ngValue]="brand.id">
              {{ brand.name }}
            </option>
          </select>
        </div>
      </div>

      <p class="message" *ngIf="errorMessage">{{ errorMessage }}</p>

      <div class="grid" *ngIf="!loading && products.length">
        <div class="card product-card" *ngFor="let product of products; trackBy: trackByProductId">
          <img
            class="product-image"
            [src]="getProductImageUrl(product)"
            [alt]="product.name"
            loading="lazy"
            decoding="async"
          />
          <h3>{{ product.name }}</h3>
          <p class="muted">{{ product.category.name }}</p>
          <div class="product-price">{{ product.price | currency }}</div>
          <p class="muted">Available now: {{ getAvailableStock(product) }}</p>
          <p class="muted" *ngIf="getQuantityInCart(product.id)">
            In your cart: {{ getQuantityInCart(product.id) }}
          </p>
          <button
            class="primary"
            type="button"
            [disabled]="getAvailableStock(product) < 1"
            (click)="addToCart(product)"
          >
            {{ getAvailableStock(product) < 1 ? 'Out of stock' : 'Add to cart' }}
          </button>
        </div>
      </div>

      <p class="muted" *ngIf="!loading && !products.length">No products match this filter.</p>
      <p class="muted" *ngIf="loading">Loading products...</p>
    </section>
  `
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  categories: Category[] = [];
  brands: Brand[] = [];
  selectedCategoryId = 0;
  selectedBrandId = 0;
  searchTerm = '';
  loading = false;
  errorMessage = '';
  private readonly cartQuantityByProduct = new Map<number, number>();
  private readonly searchInput$ = new Subject<string>();
  private cartSubscription?: Subscription;
  private searchSubscription?: Subscription;

  constructor(
    private readonly productService: ProductService,
    private readonly cartService: CartService,
    private readonly authService: AuthService,
    private readonly notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.errorMessage = '';

    this.cartSubscription = this.cartService.items$.subscribe((items) => {
      this.cartQuantityByProduct.clear();
      for (const item of items) {
        this.cartQuantityByProduct.set(item.productId, item.quantity);
      }
    });

    this.searchSubscription = this.searchInput$
      .pipe(debounceTime(250), distinctUntilChanged())
      .subscribe(() => this.loadProducts());

    this.productService.getCategories().subscribe({
      next: (categories) => (this.categories = categories),
      error: () => (this.errorMessage = 'Unable to load categories.')
    });

    this.productService.getBrands().subscribe({
      next: (brands) => (this.brands = brands),
      error: () => (this.errorMessage = 'Unable to load brands.')
    });

    this.loadProducts();
  }

  onFilterChange(): void {
    this.loadProducts();
  }

  onSearchChange(value: string): void {
    this.searchInput$.next(value);
  }

  private loadProducts(): void {
    this.loading = true;

    this.productService.getProducts({
      categoryId: this.selectedCategoryId || undefined,
      brandId: this.selectedBrandId || undefined,
      search: this.searchTerm || undefined
    }).subscribe({
      next: (products) => {
        this.products = products;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load products.';
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.cartSubscription?.unsubscribe();
    this.searchSubscription?.unsubscribe();
  }

  getQuantityInCart(productId: number): number {
    return this.cartQuantityByProduct.get(productId) ?? 0;
  }

  getAvailableStock(product: Product): number {
    return Math.max(product.stock - this.getQuantityInCart(product.id), 0);
  }

  trackByProductId(_: number, product: Product): number {
    return product.id;
  }

  getProductImageUrl(product: Product): string {
    return `https://picsum.photos/seed/product-${product.id}/420/260`;
  }

  addToCart(product: Product): void {
    if (!this.authService.isLoggedIn) {
      this.errorMessage = 'Please login to add items to cart.';
      return;
    }

    if (this.getAvailableStock(product) < 1) {
      this.errorMessage = 'This item is currently out of stock.';
      return;
    }

    this.cartService.addToCart(product.id, 1, { productName: product.name, unitPrice: product.price }).subscribe({
      next: () => {
        this.errorMessage = '';
        this.notificationService.showSuccess(`${product.name} added to cart.`);
      },
      error: () => {
        this.errorMessage = 'Unable to add item to cart.';
      }
    });
  }
}
