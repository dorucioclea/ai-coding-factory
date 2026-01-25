import { useEffect } from 'react';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import * as SplashScreen from 'expo-splash-screen';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { Provider } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';
import { QueryClientProvider } from '@tanstack/react-query';

import { store, persistor } from '@/store';
import { queryClient } from '@/services/api/queryClient';
import { ThemeProvider } from '@/theme';
import { RootErrorBoundary } from '@/observability/ErrorBoundary';
import { initializeSentry } from '@/observability/sentry.config';
import { useNavigationTracking } from '@/observability/navigationTracking';

// Keep the splash screen visible while we initialize
SplashScreen.preventAutoHideAsync();

function RootLayoutNav() {
  // Track navigation for observability
  useNavigationTracking();

  return (
    <>
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="(auth)" />
        <Stack.Screen name="(app)" />
        <Stack.Screen
          name="modal"
          options={{
            presentation: 'modal',
            headerShown: true,
          }}
        />
      </Stack>
      <StatusBar style="auto" />
    </>
  );
}

export default function RootLayout() {
  useEffect(() => {
    // Initialize Sentry
    initializeSentry();

    // Hide splash screen
    SplashScreen.hideAsync();
  }, []);

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <RootErrorBoundary>
        <Provider store={store}>
          <PersistGate loading={null} persistor={persistor}>
            <QueryClientProvider client={queryClient}>
              <ThemeProvider>
                <RootLayoutNav />
              </ThemeProvider>
            </QueryClientProvider>
          </PersistGate>
        </Provider>
      </RootErrorBoundary>
    </GestureHandlerRootView>
  );
}
