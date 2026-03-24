import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { UpdateUserRequest, User } from '../../shared/models/user';

@Injectable({ providedIn: 'root' })
export class UserService {
    private readonly apiBaseUrl = environment.apiBaseUrl;

    constructor(private readonly http: HttpClient) { }

    getUsers(): Observable<User[]> {
        return this.http
            .get<ApiResponse<User[]>>(`${this.apiBaseUrl}/api/users`)
            .pipe(map((response) => response.data ?? []));
    }

    getUser(id: number): Observable<User> {
        return this.http
            .get<ApiResponse<User>>(`${this.apiBaseUrl}/api/users/${id}`)
            .pipe(map((response) => response.data!));
    }

    updateUser(id: number, request: UpdateUserRequest): Observable<User> {
        return this.http
            .put<ApiResponse<User>>(`${this.apiBaseUrl}/api/users/${id}`, request)
            .pipe(map((response) => response.data!));
    }

    deleteUser(id: number): Observable<string> {
        return this.http
            .delete<ApiResponse<string>>(`${this.apiBaseUrl}/api/users/${id}`)
            .pipe(map((response) => response.data ?? ''));
    }
}
