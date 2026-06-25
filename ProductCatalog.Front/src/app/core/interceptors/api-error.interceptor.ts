import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error('API request failed.', error);

      // TODO: Replace console logging with a centralized user-facing error strategy.
      return throwError(() => error);
    })
  );
