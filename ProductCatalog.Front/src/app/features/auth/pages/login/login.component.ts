import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { ErrorMessageComponent } from '../../../../shared/components/error-message/error-message.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ErrorMessageComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  public readonly loginForm = new FormGroup({
    username: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    })
  });
  public readonly isSubmitting = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly infoMessage = signal<string | null>(null);

  public ngOnInit(): void {
    this.activatedRoute.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((queryParams) => {
        const reason = queryParams.get('reason');

        this.infoMessage.set(
          reason === 'session-expired' ? 'Your session has expired. Please sign in again.' : null
        );
      });

    this.loginForm.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.errorMessage.set(null);
    });
  }

  public onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { username, password } = this.loginForm.getRawValue();

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);

    this.authService
      .login(username, password)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: () => {
          const returnUrl = this.activatedRoute.snapshot.queryParamMap.get('returnUrl') || '/products';
          void this.router.navigateByUrl(returnUrl);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            error.status === 401
              ? (error.error?.message ?? 'Invalid username or password.')
              : 'Unable to sign in at the moment. Please try again.'
          );
        }
      });
  }

  public hasControlError(controlName: 'username' | 'password'): boolean {
    const control = this.loginForm.controls[controlName];

    return control.invalid && (control.dirty || control.touched);
  }
}
