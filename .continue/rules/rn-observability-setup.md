---
description: "Mobile observability with Sentry - error tracking, performance monitoring, session replay, crash reporting, source maps for React Native/Expo"
globs: ["**/*"]
---


# React Native Observability Setup

## Overview

Complete observability setup for React Native/Expo applications using Sentry. Covers error tracking, performance monitoring, crash reporting, source map configuration, and EAS Build integration.

## When to Use

- Setting up observability in new React Native projects
- Configuring Sentry for Expo managed workflow
- Adding crash reporting and error tracking
- Implementing performance monitoring
- Setting up source maps for readable stack traces

## When NOT to Use

- Backend observability (use .NET Serilog patterns)
- Analytics-only requirements (use analytics SDKs)
- Simple logging (use console + dev tools first)

---

## Full Integration Setup

### 1. Install Dependencies

```bash
npx expo install @sentry/react-native
```

### 2. Sentry Configuration

```typescript
// observability/sentry.config.ts
import * as Sentry from '@sentry/react-native';
import * as Application from 'expo-application';
import Constants from 'expo-constants';

export function initSentry() {
  Sentry.init({
    dsn: Constants.expoConfig?.extra?.sentryDsn || process.env.EXPO_PUBLIC_SENTRY_DSN,

    // Environment tagging
    environment: __DEV__ ? 'development' : 'production',

    // Release tracking (critical for source maps)
    release: `${Application.applicationId}@${Application.nativeApplicationVersion}+${Application.nativeBuildVersion}`,
    dist: Application.nativeBuildVersion ?? undefined,

    // Performance sampling
    tracesSampleRate: __DEV__ ? 1.0 : 0.2,  // 20% in production
    profilesSampleRate: 0.1,                  // 10% profiling

    // Error sampling (always 100%)
    sampleRate: 1.0,

    // Breadcrumb configuration
    maxBreadcrumbs: 100,

    // Attach screenshots on crash (iOS/Android)
    attachScreenshot: true,

    // Session tracking for crash-free rate
    enableAutoSessionTracking: true,
    sessionTrackingIntervalMillis: 30000,

    // Auto instrumentation
    enableAutoPerformanceTracing: true,
    enableNativeCrashHandling: true,

    // Integrations
    integrations: [
      new Sentry.ReactNativeTracing({
        tracingOrigins: ['localhost', 'api.yourdomain.com', /^\//],
        routingInstrumentation,
      }),
    ],

    // Filter before sending
    beforeSend(event, hint) {
      // Don't send events in development
      if (__DEV__) {
        console.log('Sentry event (dev):', event);
        return null;
      }

      // Strip PII from user object
      if (event.user) {
        delete event.user.email;
        delete event.user.ip_address;
      }

      // Filter out known non-issues
      if (event.exception?.values?.[0]?.type === 'NetworkError') {
        // Don't report network timeouts
        return null;
      }

      return event;
    },

    // Filter breadcrumbs
    beforeBreadcrumb(breadcrumb, hint) {
      // Sanitize URLs
      if (breadcrumb.category === 'fetch' && breadcrumb.data?.url) {
        breadcrumb.data.url = sanitizeUrl(breadcrumb.data.url);
      }

      // Filter out noisy breadcrumbs
      if (breadcrumb.category === 'console' && breadcrumb.level === 'debug') {
        return null;
      }

      return breadcrumb;
    },
  });
}

function sanitizeUrl(url: string): string {
  try {
    const urlObj = new URL(url);
    // Remove auth tokens from query params
    urlObj.searchParams.delete('token');
    urlObj.searchParams.delete('auth');
    return urlObj.toString();
  } catch {
    return url;
  }
}

// Navigation instrumentation (exported for use in layout)
export const routingInstrumentation = new Sentry.ReactNavigationInstrumentation();
```

### 3. App Configuration

```typescript
// app.config.ts
import { ExpoConfig, ConfigContext } from 'expo/config';

export default ({ config }: ConfigContext): ExpoConfig => ({
  ...config,
  plugins: [
    // Other plugins...
    [
      '@sentry/react-native/expo',
      {
        organization: process.env.SENTRY_ORG,
        project: process.env.SENTRY_PROJECT,
      },
    ],
  ],
  hooks: {
    postPublish: [
      {
        file: 'sentry-expo/upload-sourcemaps',
        config: {
          organization: process.env.SENTRY_ORG,
          project: process.env.SENTRY_PROJECT,
        },
      },
    ],
  },
  extra: {
    sentryDsn: process.env.EXPO_PUBLIC_SENTRY_DSN,
    // Other config...
  },
});
```

### 4. EAS Secrets

```bash
# Set up EAS secrets for source map uploads
eas secret:create --name SENTRY_AUTH_TOKEN --value your-auth-token
eas secret:create --name SENTRY_ORG --value your-org
eas secret:create --name SENTRY_PROJECT --value your-project
eas secret:create --name EXPO_PUBLIC_SENTRY_DSN --value your-dsn
```

### 5. Root Layout Integration

```typescript
// app/_layout.tsx
import { useEffect } from 'react';
import { Stack } from 'expo-router';
import { useNavigationContainerRef } from 'expo-router';
import * as Sentry from '@sentry/react-native';

import { initSentry, routingInstrumentation } from '@/observability/sentry.config';
import { RootErrorBoundary } from '@/observability/ErrorBoundary';

// Initialize Sentry
initSentry();

export default function RootLayout() {
  const navigationRef = useNavigationContainerRef();

  // Register navigation for tracking
  useEffect(() => {
    if (navigationRef) {
      routingInstrumentation.registerNavigationContainer(navigationRef);
    }
  }, [navigationRef]);

  return (
    <RootErrorBoundary>
      <Stack screenOptions={{ headerShown: false }} />
    </RootErrorBoundary>
  );
}
```

---

## Error Boundaries

### Root Error Boundary

```typescript
// observability/ErrorBoundary.tsx
import React from 'react';
import { View, Text, Pressable, StyleSheet } from 'react-native';
import * as Sentry from '@sentry/react-native';
import { ErrorBoundary as SentryErrorBoundary } from '@sentry/react-native';

interface FallbackProps {
  error: Error;
  resetError: () => void;
}

function ErrorFallback({ error, resetError }: FallbackProps) {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Something went wrong</Text>
      <Text style={styles.message}>{error.message}</Text>
      <Pressable style={styles.button} onPress={resetError}>
        <Text style={styles.buttonText}>Try Again</Text>
      </Pressable>
    </View>
  );
}

export function RootErrorBoundary({ children }: { children: React.ReactNode }) {
  return (
    <SentryErrorBoundary
      fallback={({ error, resetError }) => (
        <ErrorFallback error={error} resetError={resetError} />
      )}
      onError={(error, componentStack) => {
        Sentry.withScope((scope) => {
          scope.setTag('error.boundary', 'root');
          scope.setContext('component_stack', { stack: componentStack });
        });
      }}
    >
      {children}
    </SentryErrorBoundary>
  );
}

// Screen-level HOC
export function withScreenErrorBoundary<P extends object>(
  WrappedComponent: React.ComponentType<P>,
  screenName: string
) {
  return function ScreenWithBoundary(props: P) {
    return (
      <SentryErrorBoundary
        fallback={({ error, resetError }) => (
          <View style={styles.screenFallback}>
            <Text style={styles.title}>Screen Error</Text>
            <Text style={styles.message}>{error.message}</Text>
            <Pressable style={styles.button} onPress={resetError}>
              <Text style={styles.buttonText}>Retry</Text>
            </Pressable>
          </View>
        )}
        onError={(error) => {
          Sentry.withScope((scope) => {
            scope.setTag('error.boundary', 'screen');
            scope.setTag('screen.name', screenName);
          });
        }}
      >
        <WrappedComponent {...props} />
      </SentryErrorBoundary>
    );
  };
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
    backgroundColor: '#FAFAFA',
  },
  screenFallback: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
  },
  title: {
    fontSize: 20,
    fontWeight: '600',
    color: '#1A1A1A',
    marginBottom: 8,
  },
  message: {
    fontSize: 14,
    color: '#666666',
    textAlign: 'center',
    marginBottom: 24,
  },
  button: {
    backgroundColor: '#3B82F6',
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
  },
  buttonText: {
    color: '#FFFFFF',
    fontWeight: '600',
    fontSize: 16,
  },
});
```

---

## Performance Monitoring

### Screen Load Tracking

```typescript
// observability/useScreenTransaction.ts
import * as Sentry from '@sentry/react-native';
import { useEffect, useRef } from 'react';

export function useScreenTransaction(screenName: string) {
  const transactionRef = useRef<Sentry.Transaction | null>(null);

  useEffect(() => {
    transactionRef.current = Sentry.startTransaction({
      name: screenName,
      op: 'ui.load',
    });

    // Mark as started
    Sentry.addBreadcrumb({
      category: 'ui.load',
      message: `Started loading ${screenName}`,
      level: 'info',
    });

    return () => {
      if (transactionRef.current) {
        transactionRef.current.finish();
      }
    };
  }, [screenName]);

  return {
    // Add child span for specific operations
    startSpan: (name: string, op: string) => {
      return transactionRef.current?.startChild({ op, description: name });
    },

    // Mark when screen is interactive
    markInteractive: () => {
      if (transactionRef.current) {
        Sentry.setMeasurement('time_to_interactive', performance.now(), 'millisecond');
        Sentry.addBreadcrumb({
          category: 'ui.load',
          message: `${screenName} is now interactive`,
          level: 'info',
        });
      }
    },
  };
}

// Usage
function ProductDetailScreen() {
  const { startSpan, markInteractive } = useScreenTransaction('ProductDetail');

  useEffect(() => {
    const fetchSpan = startSpan('fetch_product', 'http.client');
    fetchProduct()
      .then(() => {
        fetchSpan?.finish();
        markInteractive();
      })
      .catch((error) => {
        fetchSpan?.setStatus('internal_error');
        fetchSpan?.finish();
      });
  }, []);

  return <View>{/* ... */}</View>;
}
```

### Network Request Instrumentation

```typescript
// services/api/instrumentedFetch.ts
import * as Sentry from '@sentry/react-native';

export async function instrumentedFetch(
  url: string,
  options?: RequestInit
): Promise<Response> {
  const span = Sentry.startSpan({
    op: 'http.client',
    name: `${options?.method || 'GET'} ${new URL(url).pathname}`,
  });

  try {
    const startTime = Date.now();
    const response = await fetch(url, options);
    const duration = Date.now() - startTime;

    span?.setHttpStatus(response.status);
    span?.setData('response_size', response.headers.get('content-length'));
    span?.setData('duration_ms', duration);

    // Add breadcrumb for non-successful responses
    if (!response.ok) {
      Sentry.addBreadcrumb({
        category: 'http',
        message: `HTTP ${response.status}: ${url}`,
        level: response.status >= 500 ? 'error' : 'warning',
        data: {
          url,
          status: response.status,
          duration,
        },
      });
    }

    return response;
  } catch (error) {
    span?.setStatus('internal_error');

    Sentry.addBreadcrumb({
      category: 'http',
      message: `Network error: ${url}`,
      level: 'error',
      data: { url, error: error.message },
    });

    throw error;
  } finally {
    span?.finish();
  }
}
```

---

## Breadcrumb Strategy

### Breadcrumb Helpers

```typescript
// observability/useBreadcrumbs.ts
import * as Sentry from '@sentry/react-native';

export const breadcrumbs = {
  // User interactions
  userAction: (action: string, data?: Record<string, unknown>) => {
    Sentry.addBreadcrumb({
      category: 'ui.click',
      message: action,
      level: 'info',
      data,
    });
  },

  // State changes
  stateChange: (description: string, data?: Record<string, unknown>) => {
    Sentry.addBreadcrumb({
      category: 'state',
      message: description,
      level: 'info',
      data,
    });
  },

  // Navigation
  navigation: (from: string, to: string) => {
    Sentry.addBreadcrumb({
      category: 'navigation',
      message: `Navigated from ${from} to ${to}`,
      level: 'info',
      data: { from, to },
    });
  },

  // App lifecycle
  lifecycle: (event: string) => {
    Sentry.addBreadcrumb({
      category: 'app.lifecycle',
      message: event,
      level: 'info',
    });
  },

  // API calls
  api: (method: string, endpoint: string, status?: number) => {
    Sentry.addBreadcrumb({
      category: 'http',
      message: `${method} ${endpoint}${status ? ` - ${status}` : ''}`,
      level: status && status >= 400 ? 'warning' : 'info',
      data: { method, endpoint, status },
    });
  },
};

// Usage examples
breadcrumbs.userAction('Tapped "Add to Cart"', { productId: '123', quantity: 2 });
breadcrumbs.stateChange('Cart updated', { itemCount: 5, total: 99.99 });
breadcrumbs.navigation('ProductList', 'ProductDetail');
breadcrumbs.lifecycle('App went to background');
```

### App Lifecycle Tracking

```typescript
// observability/useAppLifecycle.ts
import { useEffect } from 'react';
import { AppState, AppStateStatus } from 'react-native';
import * as Sentry from '@sentry/react-native';

export function useAppLifecycleBreadcrumbs() {
  useEffect(() => {
    const subscription = AppState.addEventListener(
      'change',
      (state: AppStateStatus) => {
        Sentry.addBreadcrumb({
          category: 'app.lifecycle',
          message: `App state: ${state}`,
          level: 'info',
          data: { state },
        });

        // Set tag for filtering
        Sentry.setTag('app.state', state);
      }
    );

    return () => subscription.remove();
  }, []);
}
```

---

## User Context

```typescript
// observability/userContext.ts
import * as Sentry from '@sentry/react-native';

export function setUserContext(user: {
  id: string;
  username?: string;
  tier?: string;
}) {
  Sentry.setUser({
    id: user.id,
    username: user.username,
    // Custom data
    segment: user.tier,
  });

  // Add as tag for filtering
  if (user.tier) {
    Sentry.setTag('user.tier', user.tier);
  }
}

export function clearUserContext() {
  Sentry.setUser(null);
  Sentry.setTag('user.tier', undefined);
}

// Usage in auth flow
function onLogin(user: User) {
  setUserContext({
    id: user.id,
    username: user.username,
    tier: user.subscriptionTier,
  });
}

function onLogout() {
  clearUserContext();
}
```

---

## Testing Observability

```typescript
// utils/testObservability.ts
import * as Sentry from '@sentry/react-native';

export const observabilityTests = {
  // Test JavaScript error capture
  triggerJsError: () => {
    throw new Error('Test JavaScript error from observability check');
  },

  // Test native crash (WARNING: Actually crashes the app)
  triggerNativeCrash: () => {
    Sentry.nativeCrash();
  },

  // Test manual event capture
  sendTestEvent: () => {
    Sentry.captureMessage('Test event - observability check', 'info');
  },

  // Test with extra context
  testWithContext: () => {
    Sentry.withScope((scope) => {
      scope.setTag('test.type', 'manual');
      scope.setContext('test_data', {
        timestamp: new Date().toISOString(),
        environment: __DEV__ ? 'development' : 'production',
      });
      Sentry.captureMessage('Test with context', 'info');
    });
  },
};

// Add dev menu button for testing
if (__DEV__) {
  // Can be triggered from dev menu or debug screen
}
```

---

## Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Minified stack traces | Missing source maps | Verify SENTRY_AUTH_TOKEN in EAS secrets |
| Events not appearing | Development filter | Check beforeSend allows dev events when testing |
| Release mismatch | Version format | Use exact format: `bundleId@version+buildNumber` |
| Native crashes not captured | Expo Go limitations | Test with development build (`npx expo run:ios`) |
| High event volume | Over-sampling | Set tracesSampleRate to 0.2 in production |

---

## Context7 Integration

Query Sentry documentation for latest patterns:

```
1. resolve-library-id: "sentry-react-native"
2. Query: "Sentry React Native Expo integration"
3. Query: "Sentry source maps EAS Build"
```

---

## Related Skills

- `rn-crash-instrumentation` - Advanced crash handling
- `rn-performance-monitoring` - Detailed performance patterns
- `rn-navigation` - Navigation tracking setup
- `rn-api-integration` - API instrumentation
