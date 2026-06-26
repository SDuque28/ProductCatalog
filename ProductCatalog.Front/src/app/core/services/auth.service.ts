import { HttpClient } from '@angular/common/http';
import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { AUTH_LOGIN_URL_SUFFIX, AUTH_REGISTER_URL_SUFFIX } from '../constants/auth.constants';
import { AuthState, LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from '../models';
import {
  clearStoredAuthSession,
  getStoredToken,
  readStoredAuthState,
  storeAuthSession
} from '../utils/auth-storage.util';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly httpClient = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly loginUrl = this.buildAuthUrl(AUTH_LOGIN_URL_SUFFIX);
  private readonly registerUrl = this.buildAuthUrl(AUTH_REGISTER_URL_SUFFIX);

  public login(username: string, password: string): Observable<LoginResponse> {
    const request: LoginRequest = {
      username: username.trim(),
      password
    };

    return this.httpClient.post<LoginResponse>(this.loginUrl, request).pipe(
      tap((response) => {
        storeAuthSession(this.getStorage(), response);
      })
    );
  }

  public register(payload: RegisterRequest): Observable<RegisterResponse> {
    const request: RegisterRequest = {
      username: payload.username.trim(),
      email: payload.email.trim(),
      password: payload.password,
      confirmPassword: payload.confirmPassword
    };

    return this.httpClient.post<RegisterResponse>(this.registerUrl, request);
  }

  public logout(): void {
    clearStoredAuthSession(this.getStorage());
  }

  public getToken(): string | null {
    return getStoredToken(this.getStorage());
  }

  public isAuthenticated(): boolean {
    return this.getAuthState().isAuthenticated;
  }

  public getAuthState(): AuthState {
    return readStoredAuthState(this.getStorage());
  }

  private buildAuthUrl(path: string): string {
    return `${API_BASE_URL}${path}`;
  }

  private getStorage(): Storage | null {
    return isPlatformBrowser(this.platformId) ? localStorage : null;
  }
}
