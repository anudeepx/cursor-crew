export interface PaymentCreateRequest {
    orderId: number;
}

export interface PaymentVerifyRequest {
    orderId: number;
    transactionId: string;
}

export interface PaymentResponse {
    orderId: number;
    status: string;
    transactionId: string;
    updatedAt: string;
}
