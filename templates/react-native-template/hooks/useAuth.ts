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
import type { LoginCredentials, RegisterData } from '@/types/shared/auth.types';

export function useAuth() {
  const dispatch = useAppDispatch();
  const user = useAppSelector(selectUser);
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const isLoading = useAppSelector(selectIsAuthLoading);
  const error = useAppSelector(selectAuthError);
  const isInitialized = useAppSelector(selectIsInitialized);

  return {
    // State
    user,
    isAuthenticated,
    isLoading,
    error,
    isInitialized,

    // Actions
    login: (credentials: LoginCredentials) => dispatch(login(credentials)),
    register: (data: RegisterData) => dispatch(register(data)),
    logout: () => dispatch(logout()),
    initialize: () => dispatch(initializeAuth()),
    clearError: () => dispatch(clearError()),
  };
}
