import {
  AUTH_TOKEN_EXPIRES_AT_STORAGE_KEY,
  AUTH_TOKEN_STORAGE_KEY,
  AUTH_TOKEN_TYPE_STORAGE_KEY
} from '../constants/auth.constants';
import { AuthState } from '../models/auth-state.model';
import { LoginResponse } from '../models/login-response.model';

export function readStoredAuthState(storage: Storage | null): AuthState {
  if (!storage) {
    return createEmptyAuthState();
  }

  const token = storage.getItem(AUTH_TOKEN_STORAGE_KEY);
  const expiresAt = storage.getItem(AUTH_TOKEN_EXPIRES_AT_STORAGE_KEY);

  if (!token || !expiresAt || isExpired(expiresAt)) {
    clearStoredAuthSession(storage);
    return createEmptyAuthState();
  }

  return {
    token,
    expiresAt,
    isAuthenticated: true
  };
}

export function storeAuthSession(storage: Storage | null, response: LoginResponse): void {
  if (!storage) {
    return;
  }

  storage.setItem(AUTH_TOKEN_STORAGE_KEY, response.token);
  storage.setItem(AUTH_TOKEN_EXPIRES_AT_STORAGE_KEY, response.expiresAt);
  storage.setItem(AUTH_TOKEN_TYPE_STORAGE_KEY, response.tokenType);
}

export function clearStoredAuthSession(storage: Storage | null): void {
  if (!storage) {
    return;
  }

  storage.removeItem(AUTH_TOKEN_STORAGE_KEY);
  storage.removeItem(AUTH_TOKEN_EXPIRES_AT_STORAGE_KEY);
  storage.removeItem(AUTH_TOKEN_TYPE_STORAGE_KEY);
}

export function getStoredToken(storage: Storage | null): string | null {
  return readStoredAuthState(storage).token;
}

export function getStoredTokenType(storage: Storage | null): string {
  if (!storage) {
    return 'Bearer';
  }

  const authState = readStoredAuthState(storage);

  return authState.isAuthenticated
    ? storage.getItem(AUTH_TOKEN_TYPE_STORAGE_KEY) ?? 'Bearer'
    : 'Bearer';
}

function createEmptyAuthState(): AuthState {
  return {
    token: null,
    expiresAt: null,
    isAuthenticated: false
  };
}

function isExpired(expiresAt: string): boolean {
  const expiration = Date.parse(expiresAt);

  return Number.isNaN(expiration) || expiration <= Date.now();
}
