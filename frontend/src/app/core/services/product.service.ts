import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { Category, Product } from '../../shared/models/product';

export interface Brand {
  id: number;
  name: string;
  email: string;
  phone: string;
  address: string;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly apiBaseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) { }

  getProducts(filters?: { categoryId?: number; brandId?: number; search?: string }): Observable<Product[]> {
    const query = new URLSearchParams();
    if (filters?.categoryId && filters.categoryId > 0) {
      query.set('categoryId', String(filters.categoryId));
    }
    if (filters?.brandId && filters.brandId > 0) {
      query.set('brandId', String(filters.brandId));
    }
    if (filters?.search?.trim()) {
      query.set('search', filters.search.trim());
    }

    const queryString = query.toString();
    const suffix = queryString ? `?${queryString}` : '';
    return this.http
      .get<ApiResponse<Product[]>>(`${this.apiBaseUrl}/api/products${suffix}`)
      .pipe(map((response) => response.data ?? []));
  }

  getCategories(): Observable<Category[]> {
    return this.http
      .get<ApiResponse<Category[]>>(`${this.apiBaseUrl}/api/categories`)
      .pipe(map((response) => response.data ?? []));
  }

  getBrands(): Observable<Brand[]> {
    return this.http
      .get<ApiResponse<Brand[]>>(`${this.apiBaseUrl}/api/brands`)
      .pipe(map((response) => response.data ?? []));
  }

  getProduct(id: number): Observable<Product | null> {
    return this.http
      .get<ApiResponse<Product>>(`${this.apiBaseUrl}/api/products/${id}`)
      .pipe(map((response) => response.data ?? null));
  }
}
