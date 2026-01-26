# React Native Auth Integration

## Overview

Complete authentication integration for React Native/Expo applications. Implements JWT-based auth with secure token storage, automatic refresh, and protected routing patterns that mirror the web frontend approach.

## When to Use

- Implementing login/register flows
- Setting up secure token storage
- Adding protected routes
- Implementing token refresh
- Integrating with backend JWT auth

## When NOT to Use

- Social auth only (use Expo AuthSession)
- API key authentication
- Anonymous/guest-only apps

---

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                     Auth Integration                          │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐      │
│  │  Auth UI    │    │ Auth State  │    │ Auth Storage│      │
│  │  (Screens)  │───▶│   (Redux)   │───▶│(SecureStore)│      │
│  └─────────────┘    └──────┬──────┘    └─────────────┘      │
│                            │                                  │
│                    ┌───────▼───────┐                         │
│                    │  Auth Service │                         │
│                    │    (API)      │                         │
│                    └───────────────┘                         │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                   Auth Context                           │ │
│  │  - isAuthenticated, isLoading                           │ │
│  │  - user, login(), logout(), register()                  │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

## Secure Token Storage

### Storage Service

```typescript
// services/auth/authStorage.ts
import * as SecureStore from 'expo-secure-store';
import { AuthTokens } from '@/types/auth.types';

const STORAGE_KEYS = {
  ACCESS_TOKEN: 'auth_access_token',
  REFRESH_TOKEN: 'auth_refresh_token',
  USER: 'auth_user',
} as const;

export const authStorage = {
  // Access Token
  async getAccessToken(): Promise<string | null> {
    try {
      return await SecureStore.getItemAsync(STORAGE_KEYS.ACCESS_TOKEN);
    } catch {
      return null;
    }
  },

  async setAccessToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.ACCESS_TOKEN, token);
  },

  // Refresh Token
  async getRefreshToken(): Promise<string | null> {
    try {
      return await SecureStore.getItemAsync(STORAGE_KEYS.REFRESH_TOKEN);
    } catch {
      return null;
    }
  },

  async setRefreshToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.REFRESH_TOKEN, token);
  },

  // Store both tokens
  async setTokens(tokens: AuthTokens): Promise<void> {
    await Promise.all([
      this.setAccessToken(tokens.accessToken),
      this.setRefreshToken(tokens.refreshToken),
    ]);
  },

  // Get both tokens
  async getTokens(): Promise<AuthTokens | null> {
    const [accessToken, refreshToken] = await Promise.all([
      this.getAccessToken(),
      this.getRefreshToken(),
    ]);

    if (!accessToken || !refreshToken) {
      return null;
    }

    return { accessToken, refreshToken };
  },

  // Clear all auth data
  async clear(): Promise<void> {
    await Promise.all([
      SecureStore.deleteItemAsync(STORAGE_KEYS.ACCESS_TOKEN),
      SecureStore.deleteItemAsync(STORAGE_KEYS.REFRESH_TOKEN),
      SecureStore.deleteItemAsync(STORAGE_KEYS.USER),
    ]);
  },

  // User data (for quick access without API call)
  async getUser(): Promise<User | null> {
    try {
      const userData = await SecureStore.getItemAsync(STORAGE_KEYS.USER);
      return userData ? JSON.parse(userData) : null;
    } catch {
      return null;
    }
  },

  async setUser(user: User): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.USER, JSON.stringify(user));
  },
};
```

---

## Auth Service

```typescript
// services/auth/authService.ts
import { apiClient } from '@/services/api/apiClient';
import { authStorage } from './authStorage';
import {
  User,
  AuthTokens,
  LoginCredentials,
  RegisterData,
  LoginResponse,
} from '@/types/auth.types';

class AuthService {
  private readonly basePath = '/api/auth';

  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>(
      `${this.basePath}/login`,
      credentials
    );

    const { user, tokens } = response.data;

    // Store tokens securely
    await authStorage.setTokens(tokens);
    await authStorage.setUser(user);

    return response.data;
  }

  async register(data: RegisterData): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>(
      `${this.basePath}/register`,
      data
    );

    const { user, tokens } = response.data;

    await authStorage.setTokens(tokens);
    await authStorage.setUser(user);

    return response.data;
  }

  async logout(): Promise<void> {
    try {
      // Notify backend (optional, for token invalidation)
      await apiClient.post(`${this.basePath}/logout`);
    } catch {
      // Continue with local logout even if API fails
    } finally {
      await authStorage.clear();
    }
  }

  async refreshToken(refreshToken: string): Promise<AuthTokens> {
    const response = await apiClient.post<AuthTokens>(
      `${this.basePath}/refresh`,
      { refreshToken }
    );

    const tokens = response.data;
    await authStorage.setTokens(tokens);

    return tokens;
  }

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<User>(`${this.basePath}/me`);
    await authStorage.setUser(response.data);
    return response.data;
  }

  async forgotPassword(email: string): Promise<void> {
    await apiClient.post(`${this.basePath}/forgot-password`, { email });
  }

  async resetPassword(token: string, password: string): Promise<void> {
    await apiClient.post(`${this.basePath}/reset-password`, { token, password });
  }

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await apiClient.post(`${this.basePath}/change-password`, {
      currentPassword,
      newPassword,
    });
  }
}

export const authService = new AuthService();
```

---

## Auth Context & Provider

```typescript
// services/auth/authContext.tsx
import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import * as Sentry from '@sentry/react-native';

import { authService } from './authService';
import { authStorage } from './authStorage';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import {
  login as loginAction,
  logout as logoutAction,
  setUser,
  clearError,
} from '@/slices/auth.slice';
import { User, LoginCredentials, RegisterData } from '@/types/auth.types';

interface AuthContextType {
  // State
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => Promise<void>;
  clearAuthError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const dispatch = useAppDispatch();
  const { user, isAuthenticated, isLoading, error } = useAppSelector((state) => state.auth);
  const [isInitialized, setIsInitialized] = useState(false);

  // Initialize auth state from storage
  useEffect(() => {
    async function initializeAuth() {
      try {
        const storedUser = await authStorage.getUser();
        const tokens = await authStorage.getTokens();

        if (storedUser && tokens) {
          // Validate token by fetching current user
          try {
            const currentUser = await authService.getCurrentUser();
            dispatch(setUser(currentUser));

            // Set Sentry user context
            Sentry.setUser({
              id: currentUser.id,
              username: currentUser.username,
            });
          } catch {
            // Token invalid, clear storage
            await authStorage.clear();
          }
        }
      } catch (error) {
        console.error('Auth initialization error:', error);
      } finally {
        setIsInitialized(true);
      }
    }

    initializeAuth();
  }, [dispatch]);

  const login = useCallback(async (credentials: LoginCredentials) => {
    const result = await dispatch(loginAction(credentials));

    if (loginAction.fulfilled.match(result)) {
      Sentry.setUser({
        id: result.payload.user.id,
        username: result.payload.user.username,
      });
    }
  }, [dispatch]);

  const register = useCallback(async (data: RegisterData) => {
    // Use authService directly for register
    const response = await authService.register(data);
    dispatch(setUser(response.user));

    Sentry.setUser({
      id: response.user.id,
      username: response.user.username,
    });
  }, [dispatch]);

  const logout = useCallback(async () => {
    await dispatch(logoutAction());
    Sentry.setUser(null);
  }, [dispatch]);

  const clearAuthError = useCallback(() => {
    dispatch(clearError());
  }, [dispatch]);

  // Don't render children until auth is initialized
  if (!isInitialized) {
    return null; // Or a splash screen
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated,
        isLoading,
        error,
        login,
        register,
        logout,
        clearAuthError,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
```

---

## Protected Routes

### Auth Layout

```typescript
// app/(auth)/_layout.tsx
import { Redirect, Stack } from 'expo-router';
import { useAuth } from '@/services/auth/authContext';
import { LoadingScreen } from '@/components/shared/LoadingScreen';

export default function AuthLayout() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  // Already authenticated, redirect to main app
  if (isAuthenticated) {
    return <Redirect href="/(tabs)" />;
  }

  return (
    <Stack
      screenOptions={{
        headerShown: false,
        animation: 'slide_from_right',
      }}
    >
      <Stack.Screen name="login" />
      <Stack.Screen name="register" />
      <Stack.Screen name="forgot-password" />
    </Stack>
  );
}
```

### Protected Tab Layout

```typescript
// app/(tabs)/_layout.tsx
import { Redirect, Tabs } from 'expo-router';
import { useAuth } from '@/services/auth/authContext';
import { LoadingScreen } from '@/components/shared/LoadingScreen';
import { Home, Search, User } from 'lucide-react-native';
import { colors } from '@/theme';

export default function TabLayout() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  // Not authenticated, redirect to login
  if (!isAuthenticated) {
    return <Redirect href="/login" />;
  }

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary[500],
        tabBarInactiveTintColor: colors.neutral[500],
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'Home',
          tabBarIcon: ({ color, size }) => <Home color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="search"
        options={{
          title: 'Search',
          tabBarIcon: ({ color, size }) => <Search color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Profile',
          tabBarIcon: ({ color, size }) => <User color={color} size={size} />,
        }}
      />
    </Tabs>
  );
}
```

---

## Auth Screens

### Login Screen

```typescript
// app/(auth)/login.tsx
import { View, Text, Pressable, StyleSheet, Alert } from 'react-native';
import { Link, router } from 'expo-router';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

import { useAuth } from '@/services/auth/authContext';
import { Button, Input } from '@/components/ui';
import { colors, spacing } from '@/theme';

const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginScreen() {
  const { login, isLoading, error, clearAuthError } = useAuth();

  const { control, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      await login(data);
      router.replace('/(tabs)');
    } catch (error) {
      Alert.alert('Login Failed', error.message);
    }
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Welcome Back</Text>
        <Text style={styles.subtitle}>Sign in to your account</Text>
      </View>

      <View style={styles.form}>
        <Controller
          control={control}
          name="email"
          render={({ field: { onChange, onBlur, value } }) => (
            <Input
              label="Email"
              placeholder="you@example.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.email?.message}
              testID="email-input"
            />
          )}
        />

        <Controller
          control={control}
          name="password"
          render={({ field: { onChange, onBlur, value } }) => (
            <Input
              label="Password"
              placeholder="Enter your password"
              secureTextEntry
              autoCapitalize="none"
              autoComplete="password"
              value={value}
              onChangeText={onChange}
              onBlur={onBlur}
              error={errors.password?.message}
              testID="password-input"
            />
          )}
        />

        <Link href="/forgot-password" asChild>
          <Pressable style={styles.forgotPassword}>
            <Text style={styles.forgotPasswordText}>Forgot Password?</Text>
          </Pressable>
        </Link>

        {error && (
          <Text style={styles.errorText}>{error}</Text>
        )}

        <Button
          onPress={handleSubmit(onSubmit)}
          loading={isLoading}
          disabled={isLoading}
          testID="login-button"
        >
          Sign In
        </Button>
      </View>

      <View style={styles.footer}>
        <Text style={styles.footerText}>Don't have an account?</Text>
        <Link href="/register" asChild>
          <Pressable>
            <Text style={styles.linkText}>Sign Up</Text>
          </Pressable>
        </Link>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: spacing.xl,
    backgroundColor: colors.neutral[50],
    justifyContent: 'center',
  },
  header: {
    marginBottom: spacing.xl,
  },
  title: {
    fontSize: 28,
    fontWeight: '700',
    color: colors.neutral[900],
    marginBottom: spacing.xs,
  },
  subtitle: {
    fontSize: 16,
    color: colors.neutral[500],
  },
  form: {
    gap: spacing.md,
  },
  forgotPassword: {
    alignSelf: 'flex-end',
  },
  forgotPasswordText: {
    fontSize: 14,
    color: colors.primary[500],
    fontWeight: '500',
  },
  errorText: {
    fontSize: 14,
    color: colors.semantic.error,
    textAlign: 'center',
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: spacing.xl,
    gap: spacing.xs,
  },
  footerText: {
    fontSize: 14,
    color: colors.neutral[500],
  },
  linkText: {
    fontSize: 14,
    color: colors.primary[500],
    fontWeight: '600',
  },
});
```

---

## Type Definitions

```typescript
// types/auth.types.ts
export interface User {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  avatarUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  email: string;
  password: string;
  username: string;
  firstName?: string;
  lastName?: string;
}

export interface LoginResponse {
  user: User;
  tokens: AuthTokens;
}
```

---

## Testing Auth

```typescript
// __tests__/auth/login.test.tsx
import { render, fireEvent, waitFor } from '@testing-library/react-native';
import { router } from 'expo-router';
import LoginScreen from '@/app/(auth)/login';

jest.mock('expo-router', () => ({
  router: { replace: jest.fn() },
  Link: ({ children }) => children,
}));

jest.mock('@/services/auth/authContext', () => ({
  useAuth: () => ({
    login: jest.fn().mockResolvedValue(undefined),
    isLoading: false,
    error: null,
    clearAuthError: jest.fn(),
  }),
}));

describe('LoginScreen', () => {
  it('submits valid credentials', async () => {
    const { getByTestId } = render(<LoginScreen />);

    fireEvent.changeText(getByTestId('email-input'), 'test@example.com');
    fireEvent.changeText(getByTestId('password-input'), 'password123');
    fireEvent.press(getByTestId('login-button'));

    await waitFor(() => {
      expect(router.replace).toHaveBeenCalledWith('/(tabs)');
    });
  });
});
```

---

## Context7 Integration

Query for auth patterns:

```
1. resolve-library-id: "expo-secure-store"
2. Query: "Expo SecureStore async storage"
3. Query: "React Native JWT refresh token"
```

---

## Related Skills

- `rn-api-integration` - API client with auth interceptors
- `rn-navigation` - Protected routes
- `rn-state-management` - Auth state patterns
- `rn-observability-setup` - User context tracking