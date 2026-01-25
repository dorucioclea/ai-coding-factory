import { Redirect } from 'expo-router';
import { useAppSelector } from '@/store/hooks';
import { selectIsAuthenticated, selectIsInitialized } from '@/slices/authSlice';
import { LoadingScreen } from '@/components/ui/LoadingScreen';

export default function Index() {
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const isInitialized = useAppSelector(selectIsInitialized);

  // Show loading while checking auth state
  if (!isInitialized) {
    return <LoadingScreen />;
  }

  // Redirect based on auth state
  if (isAuthenticated) {
    return <Redirect href="/(app)/(tabs)" />;
  }

  return <Redirect href="/(auth)/login" />;
}
