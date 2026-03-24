export interface CartItem {
    productId: number;
    productName: string;
    unitPrice: number;
    quantity: number;
    lineTotal: number;
}

export interface Cart {
    items: CartItem[];
    totalAmount: number;
}
