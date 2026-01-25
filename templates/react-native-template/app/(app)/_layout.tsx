import { useEffect } from 'react';
import { Redirect, Stack } from 'expo-router';
import { useAppSelector, useAppDispatch } from '@/store/hooks';
import { selectIsAuthenticated, selectIsInitialized, initializeAuth } from '@/slices/authSlice';
import { useTheme } from '@/theme';
import { LoadingScreen } from '@/components/ui/LoadingScreen';

export default function AppLayout() {
  const dispatch = useAppDispatch();
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const isInitialized = useAppSelector(selectIsInitialized);
  const { colors } = useTheme();

  useEffect(() => {
    // Initialize auth state on app load
    dispatch(initializeAuth());
  }, [dispatch]);

  // Show loading while initializing
  if (!isInitialized) {
    return <LoadingScreen />;
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Redirect href="/(auth)/login" />;
  }

  return (
    <Stack
      screenOptions={{
        headerShown: false,
        contentStyle: {
          backgroundColor: colors.background.primary,
        },
      }}
    >
      <Stack.Screen name="(tabs)" />
      <Stack.Screen
        name="settings"
        options={{
          headerShown: true,
          title: 'Settings',
        }}
      />
    </Stack>
  );
}
