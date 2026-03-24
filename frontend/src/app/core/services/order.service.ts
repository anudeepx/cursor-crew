import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { Order, OrderItemRequest } from '../../shared/models/order';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiBaseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) { }

  placeOrder(items: OrderItemRequest[]): Observable<ApiResponse<Order>> {
    return this.http.post<ApiResponse<Order>>(`${this.apiBaseUrl}/api/orders`, {
      items
    });
  }

  getOrders(): Observable<Order[]> {
    return this.http
      .get<ApiResponse<Order[]>>(`${this.apiBaseUrl}/api/orders`)
      .pipe(map((response) => response.data ?? []));
  }
}
