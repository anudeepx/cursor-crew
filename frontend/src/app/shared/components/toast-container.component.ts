import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-toast-container',
    standalone: true,
    imports: [CommonModule],
    template: `
    <section class="toast-container" aria-live="polite" aria-atomic="true">
      <article
        class="toast"
        *ngFor="let notification of notificationService.notifications$ | async"
        [class.toast-success]="notification.type === 'success'"
        [class.toast-error]="notification.type === 'error'"
        [class.toast-info]="notification.type === 'info'"
        [class.toast-warning]="notification.type === 'warning'"
      >
        <p>{{ notification.message }}</p>
        <button
          type="button"
          class="toast-close"
          (click)="notificationService.dismiss(notification.id)"
          aria-label="Dismiss notification"
        >
            x
        </button>
      </article>
    </section>
  `
})
export class ToastContainerComponent {
    constructor(readonly notificationService: NotificationService) { }
}
