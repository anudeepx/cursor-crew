export interface User {
    id: number;
    name: string;
    email: string;
    role: string;
}

export interface UpdateUserRequest {
    name: string;
    email: string;
}
