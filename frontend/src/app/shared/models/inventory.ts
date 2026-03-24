export interface InventoryItem {
    productId: number;
    productName: string;
    stockQuantity: number;
}

export interface UpdateInventoryRequest {
    stockQuantity: number;
}
