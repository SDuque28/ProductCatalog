export interface AuthState {
  token: string | null;
  expiresAt: string | null;
  isAuthenticated: boolean;
}
