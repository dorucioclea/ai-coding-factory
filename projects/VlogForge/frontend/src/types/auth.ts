/**
 * Auth types matching the backend Clean Architecture template
 * Corresponds to the JWT configuration in the .NET backend
 */

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  isEmailVerified: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: 'Bearer';
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  email: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface AuthResponse {
  success: boolean;
  data?: {
    user: User;
    tokens: AuthTokens;
  };
  error?: string;
  errors?: Record<string, string[]>;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

export type AuthStatus = 'idle' | 'loading' | 'authenticated' | 'unauthenticated';

/**
 * JWT claims from the backend CurrentUserService
 * Maps to: UserId, Email, IsAuthenticated, Roles
 */
export interface JwtPayload {
  sub: string; // User ID
  email: string;
  name?: string;
  roles: string[];
  exp: number;
  iat: number;
  iss: string;
  aud: string;
}

/**
 * Role-based access control
 * Common roles used in enterprise applications
 */
export const Roles = {
  Admin: 'Admin',
  User: 'User',
  Manager: 'Manager',
  Viewer: 'Viewer',
} as const;

export type Role = (typeof Roles)[keyof typeof Roles];
