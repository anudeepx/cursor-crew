import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PaymentCreateRequest, PaymentResponse, PaymentVerifyRequest } from '../../shared/models/payment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
    private readonly apiBaseUrl = environment.apiBaseUrl;

    constructor(private readonly http: HttpClient) { }

    createPayment(request: PaymentCreateRequest): Observable<PaymentResponse> {
        return this.http
            .post<ApiResponse<PaymentResponse>>(`${this.apiBaseUrl}/api/payments/create`, request)
            .pipe(map((response) => response.data!));
    }

    verifyPayment(request: PaymentVerifyRequest): Observable<PaymentResponse> {
        return this.http
            .post<ApiResponse<PaymentResponse>>(`${this.apiBaseUrl}/api/payments/verify`, request)
            .pipe(map((response) => response.data!));
    }

    getPaymentStatus(orderId: number): Observable<PaymentResponse> {
        return this.http
            .get<ApiResponse<PaymentResponse>>(`${this.apiBaseUrl}/api/payments/status/${orderId}`)
            .pipe(map((response) => response.data!));
    }
}
