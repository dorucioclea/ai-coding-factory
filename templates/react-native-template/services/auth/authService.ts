import { apiClient } from '@/services/api/apiClient';
import { authStorage, AuthTokens } from './authStorage';
import type {
  User,
  LoginCredentials,
  RegisterData,
  AuthResponse,
} from '@/types/shared/auth.types';

export const authService = {
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', credentials);

    await authStorage.setTokens({
      accessToken: data.tokens.accessToken,
      refreshToken: data.tokens.refreshToken,
      expiresAt: data.tokens.expiresAt,
    });

    return data;
  },

  async register(registerData: RegisterData): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/register', registerData);

    await authStorage.setTokens({
      accessToken: data.tokens.accessToken,
      refreshToken: data.tokens.refreshToken,
      expiresAt: data.tokens.expiresAt,
    });

    return data;
  },

  async logout(): Promise<void> {
    try {
      const refreshToken = await authStorage.getRefreshToken();
      if (refreshToken) {
        await apiClient.post('/auth/logout', { refreshToken });
      }
    } catch {
      // Ignore logout errors - clear storage anyway
    } finally {
      await authStorage.clearAll();
    }
  },

  async refreshTokens(): Promise<AuthTokens> {
    const refreshToken = await authStorage.getRefreshToken();

    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const { data } = await apiClient.post<{ tokens: AuthTokens }>('/auth/refresh', {
      refreshToken,
    });

    await authStorage.setTokens(data.tokens);

    return data.tokens;
  },

  async getCurrentUser(): Promise<User | null> {
    try {
      const { data } = await apiClient.get<User>('/users/me');
      return data;
    } catch {
      return null;
    }
  },

  async forgotPassword(email: string): Promise<void> {
    await apiClient.post('/auth/forgot-password', { email });
  },

  async resetPassword(token: string, password: string): Promise<void> {
    await apiClient.post('/auth/reset-password', { token, password });
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await apiClient.post('/auth/change-password', { currentPassword, newPassword });
  },
};
