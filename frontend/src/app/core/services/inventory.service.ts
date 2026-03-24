import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { InventoryItem, UpdateInventoryRequest } from '../../shared/models/inventory';

@Injectable({ providedIn: 'root' })
export class InventoryService {
    private readonly apiBaseUrl = environment.apiBaseUrl;

    constructor(private readonly http: HttpClient) { }

    getInventory(): Observable<InventoryItem[]> {
        return this.http
            .get<ApiResponse<InventoryItem[]>>(`${this.apiBaseUrl}/api/inventory`)
            .pipe(map((response) => response.data ?? []));
    }

    updateInventory(productId: number, request: UpdateInventoryRequest): Observable<InventoryItem> {
        return this.http
            .put<ApiResponse<InventoryItem>>(`${this.apiBaseUrl}/api/inventory/${productId}`, request)
            .pipe(map((response) => response.data!));
    }
}
