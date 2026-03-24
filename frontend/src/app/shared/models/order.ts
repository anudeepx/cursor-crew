export interface OrderItemRequest {
  productId: number;
  quantity: number;
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
}

export interface Order {
  id: number;
  totalAmount: number;
  status: string;
  createdAt: string;
  items: OrderItem[];
}
