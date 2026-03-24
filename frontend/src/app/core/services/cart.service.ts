import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, Observable, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { Cart, CartItem } from '../../shared/models/cart';

@Injectable({ providedIn: 'root' })
export class CartService {
    private readonly apiBaseUrl = environment.apiBaseUrl;
    private readonly emptyCart: Cart = { items: [], totalAmount: 0 };
    private readonly cartSubject = new BehaviorSubject<Cart>(this.emptyCart);
    readonly cart$ = this.cartSubject.asObservable();
    readonly items$ = this.cart$.pipe(map((cart) => cart.items));

    constructor(private readonly http: HttpClient) { }

    get items() {
        return this.cartSubject.value.items;
    }

    refreshCart(): Observable<Cart> {
        return this.http.get<ApiResponse<Cart>>(`${this.apiBaseUrl}/api/cart`).pipe(
            map((response) => response.data ?? this.emptyCart),
            tap((cart) => this.cartSubject.next(this.normalizeCart(cart)))
        );
    }

    addToCart(productId: number, quantity = 1, details?: { productName?: string; unitPrice?: number }): Observable<Cart> {
        const previousCart = this.snapshotCart();
        this.applyLocalAdd(productId, quantity, details);

        return this.http
            .post<ApiResponse<Cart>>(`${this.apiBaseUrl}/api/cart/add`, { productId, quantity })
            .pipe(
                map((response) => response.data ?? this.emptyCart),
                tap((cart) => this.cartSubject.next(this.normalizeCart(cart))),
                catchError((error) => {
                    this.cartSubject.next(previousCart);
                    return throwError(() => error);
                })
            );
    }

    updateQuantity(productId: number, quantity: number): Observable<Cart> {
        const previousCart = this.snapshotCart();
        this.applyLocalQuantityUpdate(productId, quantity);

        return this.http
            .put<ApiResponse<Cart>>(`${this.apiBaseUrl}/api/cart/update`, { productId, quantity })
            .pipe(
                map((response) => response.data ?? this.emptyCart),
                tap((cart) => this.cartSubject.next(this.normalizeCart(cart))),
                catchError((error) => {
                    this.cartSubject.next(previousCart);
                    return throwError(() => error);
                })
            );
    }

    removeFromCart(productId: number): Observable<Cart> {
        const previousCart = this.snapshotCart();
        this.applyLocalRemove(productId);

        return this.http
            .delete<ApiResponse<Cart>>(`${this.apiBaseUrl}/api/cart/remove/${productId}`)
            .pipe(
                map((response) => response.data ?? this.emptyCart),
                tap((cart) => this.cartSubject.next(this.normalizeCart(cart))),
                catchError((error) => {
                    this.cartSubject.next(previousCart);
                    return throwError(() => error);
                })
            );
    }

    clearCart(): Observable<ApiResponse<string>> {
        const previousCart = this.snapshotCart();
        this.cartSubject.next(this.emptyCart);

        return this.http.delete<ApiResponse<string>>(`${this.apiBaseUrl}/api/cart/clear`).pipe(
            catchError((error) => {
                this.cartSubject.next(previousCart);
                return throwError(() => error);
            })
        );
    }

    clearLocalCart(): void {
        this.cartSubject.next(this.emptyCart);
    }

    getTotal(): number {
        return this.cartSubject.value.totalAmount;
    }

    private snapshotCart(): Cart {
        return this.normalizeCart(this.cartSubject.value);
    }

    private applyLocalAdd(productId: number, quantity: number, details?: { productName?: string; unitPrice?: number }): void {
        const current = this.snapshotCart();
        const existing = current.items.find((item) => item.productId === productId);

        if (existing) {
            existing.quantity += quantity;
            existing.lineTotal = existing.unitPrice * existing.quantity;
        } else {
            const unitPrice = details?.unitPrice ?? 0;
            current.items.push({
                productId,
                productName: details?.productName ?? `Product #${productId}`,
                unitPrice,
                quantity,
                lineTotal: unitPrice * quantity
            });
        }

        current.totalAmount = this.calculateTotal(current.items);
        this.cartSubject.next(current);
    }

    private applyLocalQuantityUpdate(productId: number, quantity: number): void {
        const current = this.snapshotCart();
        const existing = current.items.find((item) => item.productId === productId);

        if (!existing) {
            return;
        }

        existing.quantity = quantity;
        existing.lineTotal = existing.unitPrice * quantity;
        current.totalAmount = this.calculateTotal(current.items);
        this.cartSubject.next(current);
    }

    private applyLocalRemove(productId: number): void {
        const current = this.snapshotCart();
        current.items = current.items.filter((item) => item.productId !== productId);
        current.totalAmount = this.calculateTotal(current.items);
        this.cartSubject.next(current);
    }

    private calculateTotal(items: CartItem[]): number {
        return items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
    }

    private normalizeCart(cart: Cart): Cart {
        const items = cart.items.map((item) => ({
            ...item,
            lineTotal: item.lineTotal ?? item.unitPrice * item.quantity
        }));

        return {
            items,
            totalAmount: cart.totalAmount ?? this.calculateTotal(items)
        };
    }
}
