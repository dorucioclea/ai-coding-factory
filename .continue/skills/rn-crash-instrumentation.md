# React Native Crash Instrumentation

## Overview

Advanced crash instrumentation patterns for comprehensive error tracking in React Native/Expo applications.

## When to Use

- Setting up comprehensive crash reporting
- Implementing error boundaries at multiple levels
- Adding contextual data to crash reports
- Debugging native crashes

---

## Error Boundary Hierarchy

```
┌─────────────────────────────────────────┐
│           Root Error Boundary           │
│   (Catches unhandled JS exceptions)     │
├─────────────────────────────────────────┤
│         Screen Error Boundaries         │
│   (Isolates screen-level failures)      │
├─────────────────────────────────────────┤
│       Component Error Boundaries        │
│   (Critical component isolation)        │
└─────────────────────────────────────────┘
```

---

## Root Error Boundary

```typescript
// observability/ErrorBoundary.tsx
import * as Sentry from '@sentry/react-native';
import { ErrorBoundary as SentryErrorBoundary } from '@sentry/react-native';

export function RootErrorBoundary({ children }: { children: React.ReactNode }) {
  return (
    <SentryErrorBoundary
      fallback={({ error, resetError }) => (
        <CrashScreen error={error} onRetry={resetError} />
      )}
      onError={(error, componentStack) => {
        Sentry.withScope((scope) => {
          scope.setTag('error.boundary', 'root');
          scope.setTag('error.fatal', 'true');
          scope.setContext('component_stack', { stack: componentStack });
        });
      }}
    >
      {children}
    </SentryErrorBoundary>
  );
}
```

---

## Context Enrichment

```typescript
// Add user context
Sentry.setUser({ id: user.id, username: user.username });

// Add device context
Sentry.setContext('device_state', {
  batteryLevel: await getBatteryLevel(),
  networkType: networkInfo.type,
  freeMemory: await getMemoryInfo(),
});

// Add app state context
Sentry.setContext('app_state', {
  currentScreen: navigation.getCurrentRoute(),
  cartItems: cart.items.length,
  isOnboarded: user.hasCompletedOnboarding,
});
```

---

## Testing Crashes

```typescript
export const crashTests = {
  jsError: () => { throw new Error('Test JS error'); },
  nativeCrash: () => Sentry.nativeCrash(),
  sendTestEvent: () => Sentry.captureMessage('Test event', 'info'),
};
```

---

## Context7 Integration

When uncertain about Sentry crash reporting APIs, query Context7:

```
1. Use resolve-library-id to find: "sentry-react-native"
2. Query specific topics:
   - "Sentry React Native error boundaries"
   - "Sentry scope context attachment"
   - "Sentry native crash testing"
   - "Sentry user and device context"
```

---

## Related Skills

- `rn-observability-setup` - Full Sentry setup
- `rn-performance-monitoring` - Performance tracking