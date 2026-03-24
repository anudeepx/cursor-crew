import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page">
      <h1>Login</h1>
      <p class="muted">Welcome back. Please sign in to continue.</p>

      <form class="form-grid" [formGroup]="form" (ngSubmit)="submit()">
        <label class="form-control">
          Email
          <input type="email" formControlName="email" placeholder="you@example.com" />
        </label>
        <p class="message" *ngIf="emailControl.touched && emailControl.invalid">{{ getEmailError() }}</p>

        <label class="form-control">
          Password
          <input type="password" formControlName="password" placeholder="••••••••" />
        </label>
        <p class="message" *ngIf="passwordControl.touched && passwordControl.invalid">{{ getPasswordError() }}</p>

        <button class="primary" type="submit" [disabled]="form.invalid || isSubmitting">
          {{ isSubmitting ? 'Signing in...' : 'Login' }}
        </button>
      </form>

      <p class="message" *ngIf="errorMessage">{{ errorMessage }}</p>
      <p class="muted">
        Don't have an account? <a routerLink="/register">Register</a>
      </p>
    </section>
  `
})
export class LoginComponent {
  private static readonly passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,15}$/;

  errorMessage = '';
  isSubmitting = false;
  readonly form: FormGroup<{
    email: FormControl<string>;
    password: FormControl<string>;
  }>;

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.form = this.formBuilder.nonNullable.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
      password: ['', [Validators.required, Validators.pattern(LoginComponent.passwordPattern)]]
    });
  }

  private getValidationErrorMessage(): string {
    const email = this.form.controls.email;
    const password = this.form.controls.password;

    if (email.hasError('required')) {
      return 'Email is required.';
    }

    if (email.hasError('email')) {
      return 'Please enter a valid email address.';
    }

    if (email.hasError('maxlength')) {
      return 'Email must be at most 200 characters.';
    }

    if (password.hasError('required')) {
      return 'Password is required.';
    }

    if (password.hasError('pattern')) {
      return 'Password must be 8-15 chars and include uppercase, lowercase, number, and special character.';
    }

    return 'Please correct the highlighted fields.';
  }

  get emailControl(): FormControl<string> {
    return this.form.controls.email;
  }

  get passwordControl(): FormControl<string> {
    return this.form.controls.password;
  }

  getEmailError(): string {
    if (this.emailControl.hasError('required')) {
      return 'Email is required.';
    }

    if (this.emailControl.hasError('email')) {
      return 'Please enter a valid email address.';
    }

    if (this.emailControl.hasError('maxlength')) {
      return 'Email must be at most 200 characters.';
    }

    return '';
  }

  getPasswordError(): string {
    if (this.passwordControl.hasError('required')) {
      return 'Password is required.';
    }

    if (this.passwordControl.hasError('pattern')) {
      return 'Password must be 8-15 chars and include uppercase, lowercase, number, and special character.';
    }

    return '';
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage = this.getValidationErrorMessage();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.login(this.form.getRawValue()).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (!response.success) {
          this.errorMessage = response.message || 'Login failed.';
          return;
        }
        this.router.navigate(['/products']);
      },
      error: () => {
        this.isSubmitting = false;
        this.errorMessage = 'Unable to login. Please try again.';
      }
    });
  }
}
