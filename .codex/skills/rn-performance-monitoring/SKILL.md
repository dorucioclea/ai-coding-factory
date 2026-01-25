---
name: rn-performance-monitoring
description: React Native performance - screen load tracking, network tracing, app start measurement, performance budgets
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: mobile-development
  version: "1.0.0"
  platform: react-native
external_references:
  - context7: sentry-react-native
---

# React Native Performance Monitoring

## Overview

Performance monitoring patterns for React Native applications including screen load tracking, network tracing, and app start measurement.

## When to Use

- Tracking screen load times
- Monitoring network request performance
- Measuring app startup time
- Setting and enforcing performance budgets

---

## Screen Load Tracking

```typescript
// hooks/useScreenTransaction.ts
import * as Sentry from '@sentry/react-native';
import { useEffect, useRef } from 'react';

export function useScreenTransaction(screenName: string) {
  const transactionRef = useRef<Sentry.Transaction | null>(null);

  useEffect(() => {
    transactionRef.current = Sentry.startTransaction({
      name: screenName,
      op: 'ui.load',
    });

    return () => {
      transactionRef.current?.finish();
    };
  }, [screenName]);

  return {
    startSpan: (name: string, op: string) =>
      transactionRef.current?.startChild({ op, description: name }),
    markInteractive: () => {
      Sentry.setMeasurement('time_to_interactive', performance.now(), 'millisecond');
    },
  };
}
```

---

## Network Tracing

```typescript
// Integrated with apiClient interceptors
// Tracks: request duration, response size, status codes
// Reports slow requests (>2s) as performance issues
```

---

## Performance Budgets

| Metric | Target | Warning |
|--------|--------|---------|
| App Start (Cold) | <2s | >3s |
| Screen Load | <500ms | >1s |
| API Response | <500ms | >1s |
| Bundle Size | <10MB | >15MB |

---

## Context7 Integration

When uncertain about performance monitoring APIs, query Context7:

```
1. Use resolve-library-id to find: "sentry-react-native"
2. Query specific topics:
   - "Sentry React Native transactions"
   - "Sentry performance measurements"
   - "Sentry custom spans"
   - "React Native performance profiling"
```

---

## Related Skills

- `rn-observability-setup` - Full setup
- `rn-api-integration` - Network monitoring
- `rn-crash-instrumentation` - Error tracking
