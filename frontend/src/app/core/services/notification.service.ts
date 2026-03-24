import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class NotificationService {
    private readonly notificationsSubject = new BehaviorSubject<AppNotification[]>([]);
    readonly notifications$ = this.notificationsSubject.asObservable();

    showSuccess(message: string, duration = 3000): void {
        const notification: AppNotification = {
            id: Date.now(),
            type: 'success',
            message,
            duration
        };
        this.addNotification(notification);
    }

    showError(message: string, duration = 5000): void {
        const notification: AppNotification = {
            id: Date.now(),
            type: 'error',
            message,
            duration
        };
        this.addNotification(notification);
    }

    showInfo(message: string, duration = 3000): void {
        const notification: AppNotification = {
            id: Date.now(),
            type: 'info',
            message,
            duration
        };
        this.addNotification(notification);
    }

    showWarning(message: string, duration = 4000): void {
        const notification: AppNotification = {
            id: Date.now(),
            type: 'warning',
            message,
            duration
        };
        this.addNotification(notification);
    }

    private addNotification(notification: AppNotification): void {
        const current = this.notificationsSubject.value;
        this.notificationsSubject.next([...current, notification]);

        setTimeout(() => {
            const updated = this.notificationsSubject.value.filter((n) => n.id !== notification.id);
            this.notificationsSubject.next(updated);
        }, notification.duration);
    }

    dismiss(id: number): void {
        const updated = this.notificationsSubject.value.filter((n) => n.id !== id);
        this.notificationsSubject.next(updated);
    }
}

export interface AppNotification {
    id: number;
    type: 'success' | 'error' | 'info' | 'warning';
    message: string;
    duration: number;
}
