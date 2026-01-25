import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import Constants from 'expo-constants';
import * as Sentry from '@sentry/react-native';

import { authStorage } from '@/services/auth/authStorage';
import { authService } from '@/services/auth/authService';

const API_URL = Constants.expoConfig?.extra?.apiUrl ?? 'http://localhost:5000/api';

// Extend config to include custom properties
interface CustomAxiosRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
  metadata?: {
    startTime: number;
  };
}

export const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add auth token and track timing
apiClient.interceptors.request.use(
  async (config: CustomAxiosRequestConfig) => {
    // Track request timing
    config.metadata = { startTime: Date.now() };

    // Add auth token
    const token = await authStorage.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Add breadcrumb for request
    Sentry.addBreadcrumb({
      category: 'http',
      message: `${config.method?.toUpperCase()} ${config.url}`,
      level: 'info',
      data: {
        method: config.method,
        url: config.url,
      },
    });

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle token refresh and errors
apiClient.interceptors.response.use(
  (response) => {
    const config = response.config as CustomAxiosRequestConfig;
    const duration = config.metadata?.startTime
      ? Date.now() - config.metadata.startTime
      : 0;

    // Log slow requests
    if (duration > 2000) {
      Sentry.captureMessage(`Slow API request: ${config.url}`, {
        level: 'warning',
        extra: {
          url: config.url,
          method: config.method,
          duration,
          status: response.status,
        },
      });
    }

    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as CustomAxiosRequestConfig;

    // Handle 401 - Attempt token refresh
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        await authService.refreshTokens();
        const newToken = await authStorage.getAccessToken();

        if (newToken && originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
        }

        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed - clear auth and redirect to login
        await authStorage.clearAll();

        Sentry.addBreadcrumb({
          category: 'auth',
          message: 'Token refresh failed',
          level: 'warning',
        });

        return Promise.reject(refreshError);
      }
    }

    // Log error to Sentry
    if (error.response?.status && error.response.status >= 500) {
      Sentry.captureException(error, {
        extra: {
          url: originalRequest?.url,
          method: originalRequest?.method,
          status: error.response.status,
          data: error.response.data,
        },
      });
    }

    return Promise.reject(error);
  }
);
