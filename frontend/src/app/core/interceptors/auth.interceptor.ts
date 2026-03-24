import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  if (!token) {
    return next(req);
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authReq).pipe(
    catchError((error) => {
      // Handle 401 Unauthorized - clear auth and redirect to login
      if (error.status === 401) {
        localStorage.removeItem('access_token');
        localStorage.removeItem('auth_user');
        notificationService.showWarning('Your session has expired. Please login again.');
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
