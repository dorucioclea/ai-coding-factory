# /add-mobile-auth - Add Authentication

Add JWT authentication to a React Native/Expo project.

## Usage
```
/add-mobile-auth [options]
```

Options:
- `--social` - Include social auth (Google, Apple)
- `--biometric` - Include biometric authentication
- `--mfa` - Include multi-factor authentication support
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Create Auth Storage

```typescript
// services/auth/authStorage.ts
import * as SecureStore from 'expo-secure-store';

const STORAGE_KEYS = {
  ACCESS_TOKEN: 'auth_access_token',
  REFRESH_TOKEN: 'auth_refresh_token',
  USER: 'auth_user',
} as const;

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

export const authStorage = {
  // Access Token
  async getAccessToken(): Promise<string | null> {
    return SecureStore.getItemAsync(STORAGE_KEYS.ACCESS_TOKEN);
  },

  async setAccessToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.ACCESS_TOKEN, token);
  },

  // Refresh Token
  async getRefreshToken(): Promise<string | null> {
    return SecureStore.getItemAsync(STORAGE_KEYS.REFRESH_TOKEN);
  },

  async setRefreshToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.REFRESH_TOKEN, token);
  },

  // Set both tokens
  async setTokens(tokens: AuthTokens): Promise<void> {
    await Promise.all([
      this.setAccessToken(tokens.accessToken),
      this.setRefreshToken(tokens.refreshToken),
    ]);
  },

  // Clear all auth data
  async clearAll(): Promise<void> {
    await Promise.all([
      SecureStore.deleteItemAsync(STORAGE_KEYS.ACCESS_TOKEN),
      SecureStore.deleteItemAsync(STORAGE_KEYS.REFRESH_TOKEN),
      SecureStore.deleteItemAsync(STORAGE_KEYS.USER),
    ]);
  },
};
```

### 2. Create Auth Service

```typescript
// services/auth/authService.ts
import { apiClient } from '@/services/api/apiClient';
import { authStorage, AuthTokens } from './authStorage';
import type { User, LoginCredentials, RegisterData } from '@/types/auth.types';

export interface AuthResponse {
  user: User;
  tokens: AuthTokens;
}

export const authService = {
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', credentials);
    await authStorage.setTokens(data.tokens);
    return data;
  },

  async register(data: RegisterData): Promise<AuthResponse> {
    const { data: response } = await apiClient.post<AuthResponse>('/auth/register', data);
    await authStorage.setTokens(response.tokens);
    return response;
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
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
};
```

### 3. Create Auth Redux Slice

```typescript
// slices/authSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { authService, AuthResponse } from '@/services/auth/authService';
import { authStorage } from '@/services/auth/authStorage';
import type { User, LoginCredentials, RegisterData } from '@/types/auth.types';
import type { RootState } from '@/store';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitialized: boolean;
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: false,
  isInitialized: false,
  error: null,
};

// Async Thunks
export const initializeAuth = createAsyncThunk(
  'auth/initialize',
  async (_, { rejectWithValue }) => {
    try {
      const token = await authStorage.getAccessToken();
      if (!token) return null;

      const user = await authService.getCurrentUser();
      return user;
    } catch (error) {
      await authStorage.clearAll();
      return rejectWithValue('Session expired');
    }
  }
);

export const login = createAsyncThunk(
  'auth/login',
  async (credentials: LoginCredentials, { rejectWithValue }) => {
    try {
      const response = await authService.login(credentials);
      return response;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Login failed');
    }
  }
);

export const register = createAsyncThunk(
  'auth/register',
  async (data: RegisterData, { rejectWithValue }) => {
    try {
      const response = await authService.register(data);
      return response;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Registration failed');
    }
  }
);

export const logout = createAsyncThunk('auth/logout', async () => {
  await authService.logout();
});

// Slice
const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    setUser: (state, action: PayloadAction<User>) => {
      state.user = action.payload;
    },
  },
  extraReducers: (builder) => {
    // Initialize
    builder
      .addCase(initializeAuth.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(initializeAuth.fulfilled, (state, action) => {
        state.isLoading = false;
        state.isInitialized = true;
        state.user = action.payload;
        state.isAuthenticated = !!action.payload;
      })
      .addCase(initializeAuth.rejected, (state) => {
        state.isLoading = false;
        state.isInitialized = true;
        state.isAuthenticated = false;
      });

    // Login
    builder
      .addCase(login.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload.user;
        state.isAuthenticated = true;
      })
      .addCase(login.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Register
    builder
      .addCase(register.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload.user;
        state.isAuthenticated = true;
      })
      .addCase(register.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Logout
    builder.addCase(logout.fulfilled, (state) => {
      state.user = null;
      state.isAuthenticated = false;
    });
  },
});

export const { clearError, setUser } = authSlice.actions;

// Selectors
export const selectUser = (state: RootState) => state.auth.user;
export const selectIsAuthenticated = (state: RootState) => state.auth.isAuthenticated;
export const selectIsAuthLoading = (state: RootState) => state.auth.isLoading;
export const selectAuthError = (state: RootState) => state.auth.error;
export const selectIsInitialized = (state: RootState) => state.auth.isInitialized;

export default authSlice.reducer;
```

### 4. Create Auth Hook

```typescript
// hooks/useAuth.ts
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import {
  login,
  register,
  logout,
  initializeAuth,
  clearError,
  selectUser,
  selectIsAuthenticated,
  selectIsAuthLoading,
  selectAuthError,
  selectIsInitialized,
} from '@/slices/authSlice';
import type { LoginCredentials, RegisterData } from '@/types/auth.types';

export function useAuth() {
  const dispatch = useAppDispatch();
  const user = useAppSelector(selectUser);
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const isLoading = useAppSelector(selectIsAuthLoading);
  const error = useAppSelector(selectAuthError);
  const isInitialized = useAppSelector(selectIsInitialized);

  return {
    user,
    isAuthenticated,
    isLoading,
    error,
    isInitialized,
    login: (credentials: LoginCredentials) => dispatch(login(credentials)),
    register: (data: RegisterData) => dispatch(register(data)),
    logout: () => dispatch(logout()),
    initialize: () => dispatch(initializeAuth()),
    clearError: () => dispatch(clearError()),
  };
}
```

### 5. Create Auth Screens

**Login Screen**:
```typescript
// app/(auth)/login.tsx
import { useState } from 'react';
import { View, Text, KeyboardAvoidingView, Platform } from 'react-native';
import { Link, router } from 'expo-router';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '@/hooks/useAuth';
import { Input, Button } from '@/components/ui';
import { useTheme } from '@/theme';

const loginSchema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginScreen() {
  const { login, isLoading, error } = useAuth();
  const { colors, spacing } = useTheme();

  const { control, handleSubmit, formState: { errors } } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginForm) => {
    const result = await login(data);
    if (login.fulfilled.match(result)) {
      router.replace('/(app)/(tabs)');
    }
  };

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      style={styles.container}
    >
      <View style={styles.form}>
        <Text style={styles.title}>Welcome Back</Text>

        {error && <Text style={styles.error}>{error}</Text>}

        <Controller
          control={control}
          name="email"
          render={({ field: { onChange, value } }) => (
            <Input
              label="Email"
              value={value}
              onChangeText={onChange}
              error={errors.email?.message}
              keyboardType="email-address"
              autoCapitalize="none"
            />
          )}
        />

        <Controller
          control={control}
          name="password"
          render={({ field: { onChange, value } }) => (
            <Input
              label="Password"
              value={value}
              onChangeText={onChange}
              error={errors.password?.message}
              secureTextEntry
            />
          )}
        />

        <Button
          label="Sign In"
          onPress={handleSubmit(onSubmit)}
          loading={isLoading}
        />

        <Link href="/forgot-password">
          <Text style={styles.link}>Forgot Password?</Text>
        </Link>

        <Link href="/register">
          <Text style={styles.link}>Don't have an account? Sign Up</Text>
        </Link>
      </View>
    </KeyboardAvoidingView>
  );
}
```

### 6. Update API Client with Token Refresh

```typescript
// services/api/apiClient.ts - Add interceptors
import { authStorage } from '@/services/auth/authStorage';
import { authService } from '@/services/auth/authService';

// Request interceptor - add auth token
apiClient.interceptors.request.use(async (config) => {
  const token = await authStorage.getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor - handle token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        await authService.refreshTokens();
        const newToken = await authStorage.getAccessToken();
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Redirect to login
        await authStorage.clearAll();
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);
```

## Output

```markdown
## Authentication Added

### Files Created
- `services/auth/authStorage.ts` - Secure token storage
- `services/auth/authService.ts` - Auth API calls
- `slices/authSlice.ts` - Redux auth state
- `hooks/useAuth.ts` - Auth hook
- `types/auth.types.ts` - TypeScript types
- `app/(auth)/login.tsx` - Login screen
- `app/(auth)/register.tsx` - Register screen
- `app/(auth)/forgot-password.tsx` - Forgot password screen

### Files Modified
- `services/api/apiClient.ts` - Added auth interceptors
- `store/index.ts` - Added auth reducer
- `app/(app)/_layout.tsx` - Added auth guard

### Features
- JWT token storage with expo-secure-store
- Automatic token refresh
- Protected route guards
- Login/Register/Forgot Password screens
- useAuth hook for easy access

### Usage
```typescript
import { useAuth } from '@/hooks/useAuth';

function ProfileScreen() {
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <View>
      <Text>Welcome, {user?.name}</Text>
      <Button onPress={logout} label="Sign Out" />
    </View>
  );
}
```
```
