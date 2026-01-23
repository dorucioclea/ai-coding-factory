'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useCallback } from 'react';

import { authService } from '@/lib/auth';
import { queryKeys } from '@/lib/query-client';
import type {
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  User,
  AuthResponse,
} from '@/types';

/**
 * Hook for current user data
 */
export function useUser() {
  return useQuery({
    queryKey: queryKeys.auth.user(),
    queryFn: () => authService.getCurrentUser(),
    staleTime: 1000 * 60 * 5, // 5 minutes
    retry: false,
  });
}

/**
 * Hook for auth state and operations
 */
export function useAuth() {
  const queryClient = useQueryClient();
  const router = useRouter();

  const {
    data: user,
    isLoading,
    error,
    refetch: refetchUser,
  } = useUser();

  const isAuthenticated = !!user;

  // Login mutation
  const loginMutation = useMutation({
    mutationFn: (credentials: LoginRequest) => authService.login(credentials),
    onSuccess: async (response: AuthResponse) => {
      if (response.success && response.data) {
        // Update user cache
        queryClient.setQueryData(queryKeys.auth.user(), response.data.user);
        // Redirect to dashboard
        router.push('/dashboard');
      }
    },
  });

  // Register mutation
  const registerMutation = useMutation({
    mutationFn: (data: RegisterRequest) => authService.register(data),
    onSuccess: async (response: AuthResponse) => {
      if (response.success && response.data) {
        queryClient.setQueryData(queryKeys.auth.user(), response.data.user);
        router.push('/dashboard');
      }
    },
  });

  // Logout mutation
  const logoutMutation = useMutation({
    mutationFn: () => authService.logout(),
    onSuccess: () => {
      // Clear all queries
      queryClient.clear();
      router.push('/auth/login');
    },
    onError: () => {
      // Clear even on error
      queryClient.clear();
      router.push('/auth/login');
    },
  });

  // Forgot password mutation
  const forgotPasswordMutation = useMutation({
    mutationFn: (data: ForgotPasswordRequest) =>
      authService.forgotPassword(data),
  });

  // Reset password mutation
  const resetPasswordMutation = useMutation({
    mutationFn: (data: ResetPasswordRequest) => authService.resetPassword(data),
    onSuccess: () => {
      router.push('/auth/login?reset=success');
    },
  });

  // Change password mutation
  const changePasswordMutation = useMutation({
    mutationFn: (data: ChangePasswordRequest) =>
      authService.changePassword(data),
  });

  // Verify email mutation
  const verifyEmailMutation = useMutation({
    mutationFn: (token: string) => authService.verifyEmail(token),
    onSuccess: () => {
      refetchUser();
    },
  });

  // Role checks
  const hasRole = useCallback(
    (role: string) => user?.roles?.includes(role) ?? false,
    [user]
  );

  const hasAnyRole = useCallback(
    (roles: string[]) => roles.some((role) => user?.roles?.includes(role)),
    [user]
  );

  return {
    // State
    user,
    isLoading,
    isAuthenticated,
    error,

    // Actions
    login: loginMutation.mutateAsync,
    register: registerMutation.mutateAsync,
    logout: logoutMutation.mutate,
    forgotPassword: forgotPasswordMutation.mutateAsync,
    resetPassword: resetPasswordMutation.mutateAsync,
    changePassword: changePasswordMutation.mutateAsync,
    verifyEmail: verifyEmailMutation.mutateAsync,
    refetchUser,

    // Mutation states
    isLoggingIn: loginMutation.isPending,
    isRegistering: registerMutation.isPending,
    isLoggingOut: logoutMutation.isPending,

    // Role helpers
    hasRole,
    hasAnyRole,

    // Errors
    loginError: loginMutation.error,
    registerError: registerMutation.error,
  };
}

/**
 * Hook for protecting routes that require authentication
 */
export function useRequireAuth(redirectTo = '/auth/login') {
  const router = useRouter();
  const { user, isLoading, isAuthenticated } = useAuth();

  // Redirect if not authenticated after loading
  if (!isLoading && !isAuthenticated) {
    router.push(redirectTo);
  }

  return {
    user: user as User | null,
    isLoading,
    isAuthenticated,
  };
}

/**
 * Hook for protecting routes that require specific roles
 */
export function useRequireRole(
  roles: string | string[],
  redirectTo = '/dashboard'
) {
  const router = useRouter();
  const { user, isLoading, hasRole, hasAnyRole } = useAuth();

  const requiredRoles = Array.isArray(roles) ? roles : [roles];
  const hasRequiredRole = hasAnyRole(requiredRoles);

  // Redirect if doesn't have required role after loading
  if (!isLoading && !hasRequiredRole) {
    router.push(redirectTo);
  }

  return {
    user,
    isLoading,
    hasRole,
  };
}

export default useAuth;
