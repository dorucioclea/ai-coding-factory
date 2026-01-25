import * as Sentry from '@sentry/react-native';
import * as Application from 'expo-application';
import Constants from 'expo-constants';

export function initializeSentry() {
  const dsn = Constants.expoConfig?.extra?.sentryDsn;

  if (!dsn) {
    if (__DEV__) {
      console.log('Sentry DSN not configured - skipping initialization');
    }
    return;
  }

  Sentry.init({
    dsn,

    // Environment and release tracking
    environment: __DEV__ ? 'development' : 'production',
    release: `${Application.applicationId}@${Application.nativeApplicationVersion}+${Application.nativeBuildVersion}`,
    ...(Application.nativeBuildVersion && { dist: Application.nativeBuildVersion }),

    // Performance monitoring
    tracesSampleRate: __DEV__ ? 1.0 : 0.2,
    profilesSampleRate: __DEV__ ? 1.0 : 0.1,

    // Features
    attachScreenshot: true,
    attachStacktrace: true,
    enableAutoSessionTracking: true,
    enableNative: true,
    enableNativeCrashHandling: true,

    // Auto instrumentation
    enableAutoPerformanceTracing: true,
    enableNativeFramesTracking: true,

    // Privacy - don't send PII by default
    sendDefaultPii: false,

    // Filter events
    beforeSend: (event) => {
      // Don't send events in development
      if (__DEV__) {
        console.log('Sentry event (dev mode):', event.exception?.values?.[0]?.type);
        return null;
      }

      // Filter out network errors (often transient)
      if (event.exception?.values?.[0]?.type === 'NetworkError') {
        return null;
      }

      return event;
    },

    // Filter breadcrumbs
    beforeBreadcrumb: (breadcrumb) => {
      // Filter out noisy console breadcrumbs in production
      if (!__DEV__ && breadcrumb.category === 'console' && breadcrumb.level === 'debug') {
        return null;
      }
      return breadcrumb;
    },
  });
}

// Helper to capture errors with additional context
export function captureError(error: Error, context?: Record<string, unknown>) {
  Sentry.withScope((scope) => {
    if (context) {
      scope.setContext('additional', context);
    }
    Sentry.captureException(error);
  });
}

// Helper to set user context
export function setUserContext(user: { id: string; name?: string; email?: string } | null) {
  if (user) {
    Sentry.setUser({
      id: user.id,
      ...(user.name && { username: user.name }),
      // Don't set email unless you've configured sendDefaultPii
    });
  } else {
    Sentry.setUser(null);
  }
}
