import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, timer } from 'rxjs';
import { RegisterRequest } from '../../../../core/models';
import { AuthService } from '../../../../core/services/auth.service';
import { ErrorMessageComponent } from '../../../../shared/components/error-message/error-message.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ErrorMessageComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  public readonly registerForm = new FormGroup(
    {
      username: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      email: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.email]
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(8)]
      }),
      confirmPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      })
    },
    {
      validators: [passwordMatchValidator('password', 'confirmPassword')]
    }
  );
  public readonly isSubmitting = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly successMessage = signal<string | null>(null);

  public constructor() {
    this.registerForm.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.errorMessage.set(null);
    });
  }

  public onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const payload: RegisterRequest = this.registerForm.getRawValue();

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authService
      .register(payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: (response) => {
          this.successMessage.set(response.message || 'Account created successfully. Please sign in.');
          this.registerForm.reset({
            username: '',
            email: '',
            password: '',
            confirmPassword: ''
          });

          timer(1500)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
              void this.router.navigate(['/login']);
            });
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(resolveRegisterErrorMessage(error));
        }
      });
  }

  public hasControlError(controlName: RegisterControlName): boolean {
    const control = this.registerForm.controls[controlName];

    return control.invalid && (control.dirty || control.touched);
  }

  public hasPasswordMismatch(): boolean {
    return !!this.registerForm.errors?.['passwordMismatch']
      && (this.registerForm.controls.confirmPassword.dirty
        || this.registerForm.controls.confirmPassword.touched);
  }
}

type RegisterControlName = 'username' | 'email' | 'password' | 'confirmPassword';

function passwordMatchValidator(
  passwordControlName: RegisterControlName,
  confirmPasswordControlName: RegisterControlName
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get(passwordControlName)?.value;
    const confirmPassword = control.get(confirmPasswordControlName)?.value;

    return password && confirmPassword && password !== confirmPassword
      ? { passwordMismatch: true }
      : null;
  };
}

function resolveRegisterErrorMessage(error: HttpErrorResponse): string {
  if (error.status === 409) {
    return error.error?.message ?? 'The username or email is already registered.';
  }

  if (error.status === 400) {
    return error.error?.message ?? 'Please review the registration details and try again.';
  }

  return 'Unable to create the account at the moment. Please try again later.';
}
