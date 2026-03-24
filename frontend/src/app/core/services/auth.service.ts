import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from '../../shared/models/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'access_token';
  private readonly userKey = 'auth_user';
  private readonly apiBaseUrl = environment.apiBaseUrl;
  private readonly userSubject = new BehaviorSubject<AuthUser | null>(this.loadUser());

  constructor(private readonly http: HttpClient) { }

  get user$(): Observable<AuthUser | null> {
    return this.userSubject.asObservable();
  }

  get currentUser(): AuthUser | null {
    return this.userSubject.value;
  }

  get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  get isLoggedIn(): boolean {
    return !!this.token;
  }

  login(payload: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${this.apiBaseUrl}/api/auth/login`, payload)
      .pipe(tap((response) => this.handleAuthResponse(response)));
  }

  register(payload: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${this.apiBaseUrl}/api/auth/register`, payload)
      .pipe(tap((response) => this.handleAuthResponse(response)));
  }

  me(): Observable<ApiResponse<AuthResponse>> {
    return this.http.get<ApiResponse<AuthResponse>>(`${this.apiBaseUrl}/api/auth/me`).pipe(
      tap((response) => {
        if (!response.success || !response.data) {
          return;
        }

        const user: AuthUser = {
          id: response.data.id,
          name: response.data.name,
          email: response.data.email,
          role: response.data.role
        };
        localStorage.setItem(this.userKey, JSON.stringify(user));
        this.userSubject.next(user);
      })
    );
  }

  logoutRequest(): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiBaseUrl}/api/auth/logout`, {});
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.userSubject.next(null);
  }

  private handleAuthResponse(response: ApiResponse<AuthResponse>): void {
    if (!response.success || !response.data) {
      return;
    }

    localStorage.setItem(this.tokenKey, response.data.token);
    const user: AuthUser = {
      id: response.data.id,
      name: response.data.name,
      email: response.data.email,
      role: response.data.role
    };
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.userSubject.next(user);
  }

  private loadUser(): AuthUser | null {
    const stored = localStorage.getItem(this.userKey);
    if (!stored) {
      return null;
    }

    try {
      const user = JSON.parse(stored) as Partial<AuthUser>;
      if (!user.id || !user.name || !user.email) {
        return null;
      }

      return {
        id: user.id,
        name: user.name,
        email: user.email,
        role: user.role === 'Admin' ? 'Admin' : 'User'
      };
    } catch {
      return null;
    }
  }
}
