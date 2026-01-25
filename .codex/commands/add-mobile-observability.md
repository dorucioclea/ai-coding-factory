# /add-mobile-observability - Add Observability

Add full Sentry observability stack to a React Native/Expo project.

## Usage
```
/add-mobile-observability [options]
```

Options:
- `--dsn <dsn>` - Sentry DSN (will prompt if not provided)
- `--performance` - Enable performance monitoring
- `--replay` - Enable session replay
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Install Dependencies

```bash
npx expo install @sentry/react-native
```

### 2. Create Sentry Configuration

```typescript
// observability/sentry.config.ts
import * as Sentry from '@sentry/react-native';
import * as Application from 'expo-application';
import Constants from 'expo-constants';

export function initializeSentry() {
  const dsn = Constants.expoConfig?.extra?.sentryDsn;

  if (!dsn) {
    console.warn('Sentry DSN not configured');
    return;
  }

  Sentry.init({
    dsn,

    // Environment
    environment: __DEV__ ? 'development' : 'production',

    // Release tracking
    release: `${Application.applicationId}@${Application.nativeApplicationVersion}+${Application.nativeBuildVersion}`,
    dist: Application.nativeBuildVersion ?? undefined,

    // Performance Monitoring
    tracesSampleRate: __DEV__ ? 1.0 : 0.2,
    profilesSampleRate: __DEV__ ? 1.0 : 0.1,

    // Features
    attachScreenshot: true,
    attachStacktrace: true,
    enableAutoSessionTracking: true,
    enableNative: true,
    enableNativeCrashHandling: true,

    // Privacy
    sendDefaultPii: false,

    // Auto instrumentation
    enableAutoPerformanceTracing: true,
    enableNativeFramesTracking: true,

    // Filtering
    beforeSend: (event, hint) => {
      // Filter out known non-issues
      if (event.exception?.values?.[0]?.type === 'NetworkError') {
        // Log network errors but don't send to Sentry
        console.log('Network error:', hint.originalException);
        return null;
      }

      // Don't send in development
      if (__DEV__) {
        console.log('Sentry event (dev):', event);
        return null;
      }

      return event;
    },

    // Breadcrumb filtering
    beforeBreadcrumb: (breadcrumb) => {
      // Filter out noisy breadcrumbs
      if (breadcrumb.category === 'console' && breadcrumb.level === 'debug') {
        return null;
      }
      return breadcrumb;
    },
  });
}

// Helper to capture exceptions with context
export function captureError(
  error: Error,
  context?: Record<string, unknown>
) {
  Sentry.withScope((scope) => {
    if (context) {
      scope.setContext('additional', context);
    }
    Sentry.captureException(error);
  });
}
```

### 3. Create Error Boundaries

```typescript
// observability/ErrorBoundary.tsx
import React, { Component, ReactNode } from 'react';
import { View, Text, Button, StyleSheet } from 'react-native';
import * as Sentry from '@sentry/react-native';
import { ErrorBoundary as SentryErrorBoundary } from '@sentry/react-native';

// Root Error Boundary
export function RootErrorBoundary({ children }: { children: ReactNode }) {
  return (
    <SentryErrorBoundary
      fallback={({ error, resetError }) => (
        <CrashScreen error={error} onRetry={resetError} />
      )}
      onError={(error, componentStack) => {
        Sentry.withScope((scope) => {
          scope.setTag('error.boundary', 'root');
          scope.setTag('error.fatal', 'true');
          scope.setContext('react', { componentStack });
        });
      }}
    >
      {children}
    </SentryErrorBoundary>
  );
}

// Screen Error Boundary
interface ScreenErrorBoundaryProps {
  screenName: string;
  children: ReactNode;
  fallback?: ReactNode;
}

interface ScreenErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ScreenErrorBoundary extends Component<
  ScreenErrorBoundaryProps,
  ScreenErrorBoundaryState
> {
  state: ScreenErrorBoundaryState = { hasError: false, error: null };

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    Sentry.withScope((scope) => {
      scope.setTag('error.boundary', 'screen');
      scope.setTag('screen.name', this.props.screenName);
      scope.setContext('react', { componentStack: errorInfo.componentStack });
      Sentry.captureException(error);
    });
  }

  handleRetry = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      return (
        this.props.fallback ?? (
          <ScreenErrorFallback
            error={this.state.error}
            onRetry={this.handleRetry}
          />
        )
      );
    }
    return this.props.children;
  }
}

// Crash Screen Component
function CrashScreen({
  error,
  onRetry,
}: {
  error: Error;
  onRetry: () => void;
}) {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Something went wrong</Text>
      <Text style={styles.message}>
        We've been notified and are working on a fix.
      </Text>
      <Button title="Try Again" onPress={onRetry} />
    </View>
  );
}

// Screen Error Fallback
function ScreenErrorFallback({
  error,
  onRetry,
}: {
  error: Error | null;
  onRetry: () => void;
}) {
  return (
    <View style={styles.screenError}>
      <Text style={styles.errorTitle}>Unable to load</Text>
      <Text style={styles.errorMessage}>
        There was a problem loading this screen.
      </Text>
      <Button title="Retry" onPress={onRetry} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
    backgroundColor: '#FFFFFF',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 12,
  },
  message: {
    fontSize: 16,
    textAlign: 'center',
    marginBottom: 24,
    color: '#666',
  },
  screenError: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
  },
  errorTitle: {
    fontSize: 18,
    fontWeight: '600',
    marginBottom: 8,
  },
  errorMessage: {
    fontSize: 14,
    color: '#666',
    marginBottom: 16,
  },
});
```

### 4. Create Performance Hooks

```typescript
// observability/useScreenTransaction.ts
import { useEffect, useRef, useCallback } from 'react';
import * as Sentry from '@sentry/react-native';

export function useScreenTransaction(screenName: string) {
  const transactionRef = useRef<ReturnType<typeof Sentry.startTransaction> | null>(null);
  const startTime = useRef(performance.now());

  useEffect(() => {
    transactionRef.current = Sentry.startTransaction({
      name: screenName,
      op: 'ui.load',
    });

    return () => {
      transactionRef.current?.finish();
    };
  }, [screenName]);

  const startSpan = useCallback(
    (name: string, op: string) => {
      return transactionRef.current?.startChild({
        op,
        description: name,
      });
    },
    []
  );

  const markInteractive = useCallback(() => {
    const tti = performance.now() - startTime.current;
    Sentry.setMeasurement('time_to_interactive', tti, 'millisecond');

    Sentry.addBreadcrumb({
      category: 'ui.lifecycle',
      message: `${screenName} became interactive`,
      level: 'info',
      data: { tti },
    });
  }, [screenName]);

  return { startSpan, markInteractive };
}
```

### 5. Create Breadcrumb Hooks

```typescript
// observability/useBreadcrumbs.ts
import { useCallback } from 'react';
import * as Sentry from '@sentry/react-native';

export function useBreadcrumbs() {
  const trackAction = useCallback(
    (action: string, data?: Record<string, unknown>) => {
      Sentry.addBreadcrumb({
        category: 'user.action',
        message: action,
        level: 'info',
        data: {
          ...data,
          timestamp: new Date().toISOString(),
        },
      });
    },
    []
  );

  const trackNavigation = useCallback(
    (from: string, to: string) => {
      Sentry.addBreadcrumb({
        category: 'navigation',
        message: `Navigate from ${from} to ${to}`,
        level: 'info',
        data: { from, to },
      });
    },
    []
  );

  const trackStateChange = useCallback(
    (stateName: string, oldValue: unknown, newValue: unknown) => {
      Sentry.addBreadcrumb({
        category: 'state.change',
        message: `${stateName} changed`,
        level: 'info',
        data: { stateName, oldValue, newValue },
      });
    },
    []
  );

  const trackNetworkRequest = useCallback(
    (method: string, url: string, status: number, duration: number) => {
      Sentry.addBreadcrumb({
        category: 'http',
        message: `${method} ${url}`,
        level: status >= 400 ? 'warning' : 'info',
        data: { method, url, status, duration },
      });
    },
    []
  );

  return {
    trackAction,
    trackNavigation,
    trackStateChange,
    trackNetworkRequest,
  };
}
```

### 6. Create Navigation Tracking

```typescript
// observability/navigationTracking.ts
import { useEffect } from 'react';
import { usePathname, useSegments } from 'expo-router';
import * as Sentry from '@sentry/react-native';

export function useNavigationTracking() {
  const pathname = usePathname();
  const segments = useSegments();

  useEffect(() => {
    const routeName = segments.join('/') || 'index';

    Sentry.addBreadcrumb({
      category: 'navigation',
      message: `Navigated to ${routeName}`,
      level: 'info',
      data: {
        pathname,
        segments,
        timestamp: new Date().toISOString(),
      },
    });

    // Set current route as Sentry tag for context
    Sentry.setTag('route', routeName);
  }, [pathname, segments]);
}
```

### 7. Update App Config for Source Maps

```typescript
// app.config.ts
export default {
  expo: {
    // ... other config
    plugins: [
      [
        '@sentry/react-native/expo',
        {
          organization: process.env.SENTRY_ORG,
          project: process.env.SENTRY_PROJECT,
          url: 'https://sentry.io/',
        },
      ],
    ],
    extra: {
      sentryDsn: process.env.SENTRY_DSN,
    },
  },
};
```

### 8. Update EAS Configuration

```json
// eas.json
{
  "build": {
    "production": {
      "env": {
        "SENTRY_AUTH_TOKEN": "@sentry-auth-token"
      }
    }
  }
}
```

### 9. Create Test Utilities

```typescript
// observability/testUtils.ts
import * as Sentry from '@sentry/react-native';

export const crashTests = {
  jsError: () => {
    throw new Error('Test JS error');
  },

  nativeCrash: () => {
    Sentry.nativeCrash();
  },

  sendTestEvent: () => {
    Sentry.captureMessage('Test event from mobile app', 'info');
  },

  testBreadcrumbs: () => {
    Sentry.addBreadcrumb({
      category: 'test',
      message: 'Test breadcrumb',
      level: 'info',
    });
    Sentry.captureMessage('Test with breadcrumbs', 'info');
  },
};
```

## Output

```markdown
## Observability Added

### Files Created
- `observability/sentry.config.ts` - Sentry initialization
- `observability/ErrorBoundary.tsx` - Error boundary components
- `observability/useScreenTransaction.ts` - Screen performance hook
- `observability/useBreadcrumbs.ts` - Breadcrumb tracking hook
- `observability/navigationTracking.ts` - Navigation tracking
- `observability/testUtils.ts` - Test utilities

### Files Modified
- `app.config.ts` - Added Sentry plugin
- `app/_layout.tsx` - Added initialization
- `eas.json` - Added Sentry auth token

### Environment Variables Required
```
SENTRY_DSN=https://xxx@xxx.ingest.sentry.io/xxx
SENTRY_ORG=your-org
SENTRY_PROJECT=your-project
SENTRY_AUTH_TOKEN=your-auth-token
```

### Usage
```typescript
// In screens
import { useScreenTransaction } from '@/observability/useScreenTransaction';

function MyScreen() {
  const { markInteractive } = useScreenTransaction('MyScreen');

  useEffect(() => {
    // After content loads
    markInteractive();
  }, []);
}
```

### Verification
Run test crash in development:
```typescript
import { crashTests } from '@/observability/testUtils';
crashTests.sendTestEvent(); // Check Sentry dashboard
```
```
