export interface Category {
  id: number;
  name: string;
}

export interface Seller {
  id: number;
  name: string;
  email: string;
  phone: string;
  address: string;
}

export interface Product {
  id: number;
  name: string;
  price: number;
  stock: number;
  category: Category;
  seller: Seller;
}
