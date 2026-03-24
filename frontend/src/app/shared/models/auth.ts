export interface AuthUser {
  id: number;
  name: string;
  email: string;
  role: 'Admin' | 'User';
}

export interface AuthResponse extends AuthUser {
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}
