import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const notificationService = inject(NotificationService);

    return next(req).pipe(
        catchError((error) => {
            const errorMessage = error.error?.message || error.message || 'An error occurred';

            // Don't show error for 401 (handled by auth interceptor)
            if (error.status !== 401) {
                notificationService.showError(errorMessage);
            }

            return throwError(() => error);
        })
    );
};
