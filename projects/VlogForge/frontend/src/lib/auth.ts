import { apiClient, tokenStorage } from './api-client';
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  User,
  JwtPayload,
} from '@/types';

/**
 * Decode JWT payload without verification
 * Note: Token signature is verified by the backend
 */
function decodeJwt(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const payload = parts[1];
    if (!payload) return null;

    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded) as JwtPayload;
  } catch {
    return null;
  }
}

/**
 * Check if token is expired
 */
function isTokenExpired(token: string): boolean {
  const payload = decodeJwt(token);
  if (!payload) return true;

  // Add 10 second buffer for clock skew
  return Date.now() >= (payload.exp * 1000) - 10000;
}

/**
 * Auth service for authentication operations
 * Matches backend AuthController endpoints
 */
export const authService = {
  /**
   * Login with email and password
   */
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>(
      '/auth/login',
      credentials
    );

    if (response.success && response.data) {
      const { tokens } = response.data;
      tokenStorage.setAccessToken(tokens.accessToken, tokens.expiresIn);
      tokenStorage.setRefreshToken(tokens.refreshToken);
    }

    return response;
  },

  /**
   * Register new user
   */
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/register', data);

    if (response.success && response.data) {
      const { tokens } = response.data;
      tokenStorage.setAccessToken(tokens.accessToken, tokens.expiresIn);
      tokenStorage.setRefreshToken(tokens.refreshToken);
    }

    return response;
  },

  /**
   * Logout - clear tokens and notify backend
   */
  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
    } finally {
      tokenStorage.clearTokens();
    }
  },

  /**
   * Get current user profile
   */
  async getCurrentUser(): Promise<User | null> {
    const token = tokenStorage.getAccessToken();
    if (!token || isTokenExpired(token)) {
      return null;
    }

    try {
      const response = await apiClient.get<{ success: boolean; data: User }>(
        '/auth/me'
      );
      return response.success ? response.data : null;
    } catch {
      return null;
    }
  },

  /**
   * Request password reset email
   */
  async forgotPassword(data: ForgotPasswordRequest): Promise<{ success: boolean }> {
    return apiClient.post<{ success: boolean }>('/auth/forgot-password', data);
  },

  /**
   * Reset password with token
   */
  async resetPassword(data: ResetPasswordRequest): Promise<{ success: boolean }> {
    return apiClient.post<{ success: boolean }>('/auth/reset-password', data);
  },

  /**
   * Change password for authenticated user
   */
  async changePassword(data: ChangePasswordRequest): Promise<{ success: boolean }> {
    return apiClient.post<{ success: boolean }>('/auth/change-password', data);
  },

  /**
   * Verify email with token
   */
  async verifyEmail(token: string): Promise<{ success: boolean }> {
    return apiClient.post<{ success: boolean }>('/auth/verify-email', { token });
  },

  /**
   * Resend verification email
   */
  async resendVerificationEmail(): Promise<{ success: boolean }> {
    return apiClient.post<{ success: boolean }>('/auth/resend-verification');
  },

  /**
   * Check if user is authenticated (client-side check)
   */
  isAuthenticated(): boolean {
    const token = tokenStorage.getAccessToken();
    return !!token && !isTokenExpired(token);
  },

  /**
   * Get user info from token (without API call)
   */
  getUserFromToken(): Partial<User> | null {
    const token = tokenStorage.getAccessToken();
    if (!token) return null;

    const payload = decodeJwt(token);
    if (!payload) return null;

    return {
      id: payload.sub,
      email: payload.email,
      roles: payload.roles,
    };
  },

  /**
   * Check if current user has specific role
   */
  hasRole(role: string): boolean {
    const user = this.getUserFromToken();
    return user?.roles?.includes(role) ?? false;
  },

  /**
   * Check if current user has any of the specified roles
   */
  hasAnyRole(roles: string[]): boolean {
    const user = this.getUserFromToken();
    return roles.some((role) => user?.roles?.includes(role));
  },
};

export default authService;
