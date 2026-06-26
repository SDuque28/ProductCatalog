import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { AUTH_LOGIN_URL_SUFFIX, AUTH_REGISTER_URL_SUFFIX } from '../constants/auth.constants';
import {
  clearStoredAuthSession,
  getStoredToken,
  getStoredTokenType
} from '../utils/auth-storage.util';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const storage = isPlatformBrowser(platformId) ? localStorage : null;
  const loginUrl = `${API_BASE_URL}${AUTH_LOGIN_URL_SUFFIX}`;
  const registerUrl = `${API_BASE_URL}${AUTH_REGISTER_URL_SUFFIX}`;
  const isApiRequest = request.url.startsWith(API_BASE_URL);
  const isPublicAuthRequest = request.url === loginUrl || request.url === registerUrl;
  const token = isApiRequest && !isPublicAuthRequest ? getStoredToken(storage) : null;

  const authenticatedRequest = token
    ? request.clone({
        setHeaders: {
          Authorization: `${getStoredTokenType(storage)} ${token}`
        }
      })
    : request;

  return next(authenticatedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && isApiRequest && !isPublicAuthRequest) {
        clearStoredAuthSession(storage);
        void router.navigate(['/login'], {
          queryParams: {
            reason: 'session-expired'
          }
        });
      }

      return throwError(() => error);
    })
  );
};
