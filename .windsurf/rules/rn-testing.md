<!-- React Native testing - Jest unit tests, React Native Testing Library, Detox E2E, Maestro flows for Expo apps -->


# React Native Testing

## Overview

Comprehensive testing patterns for React Native/Expo applications including unit tests with Jest, component tests with React Native Testing Library, and E2E tests with Detox and Maestro.

## When to Use

- Writing unit tests for utilities and hooks
- Testing React Native components
- Creating E2E test flows
- Setting up CI testing pipelines
- Implementing TDD for mobile features

---

## Test Pyramid

```
        /\
       /  \    E2E (Detox/Maestro)
      /----\   - Critical user flows
     /      \  - Cross-screen navigation
    /--------\ Integration (RTL + MSW)
   /          \ - Component + API mocking
  /------------\ Unit (Jest)
 /              \ - Pure functions
/----------------\ - Hooks, utilities
```

---

## Jest Configuration

```javascript
// jest.config.js
module.exports = {
  preset: 'jest-expo',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.js'],
  transformIgnorePatterns: [
    'node_modules/(?!((jest-)?react-native|@react-native(-community)?)|expo(nent)?|@expo(nent)?/.*|@expo-google-fonts/.*|react-navigation|@react-navigation/.*|@unimodules/.*|unimodules|sentry-expo|native-base|react-native-svg)',
  ],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/$1',
  },
  collectCoverageFrom: [
    '**/*.{ts,tsx}',
    '!**/node_modules/**',
    '!**/coverage/**',
    '!**/*.d.ts',
    '!**/types/**',
  ],
  coverageThreshold: {
    global: {
      branches: 80,
      functions: 80,
      lines: 80,
      statements: 80,
    },
  },
};
```

```javascript
// jest.setup.js
import '@testing-library/react-native/extend-expect';

// Mock Expo modules
jest.mock('expo-secure-store', () => ({
  getItemAsync: jest.fn(),
  setItemAsync: jest.fn(),
  deleteItemAsync: jest.fn(),
}));

jest.mock('@sentry/react-native', () => ({
  init: jest.fn(),
  captureException: jest.fn(),
  captureMessage: jest.fn(),
  setUser: jest.fn(),
  addBreadcrumb: jest.fn(),
  withScope: jest.fn((fn) => fn({ setTag: jest.fn(), setContext: jest.fn() })),
}));

// Silence console warnings in tests
jest.spyOn(console, 'warn').mockImplementation(() => {});
```

---

## Unit Testing

### Utility Functions

```typescript
// utils/__tests__/formatters.test.ts
import { formatCurrency, formatDate, truncateText } from '../formatters';

describe('formatCurrency', () => {
  it('formats USD correctly', () => {
    expect(formatCurrency(1234.56, 'USD')).toBe('$1,234.56');
  });

  it('handles zero', () => {
    expect(formatCurrency(0, 'USD')).toBe('$0.00');
  });

  it('handles negative values', () => {
    expect(formatCurrency(-50, 'USD')).toBe('-$50.00');
  });
});

describe('formatDate', () => {
  it('formats date with default format', () => {
    const date = new Date('2024-01-15T10:30:00Z');
    expect(formatDate(date)).toBe('Jan 15, 2024');
  });

  it('handles relative dates', () => {
    const now = new Date();
    expect(formatDate(now, 'relative')).toBe('Today');
  });
});

describe('truncateText', () => {
  it('truncates long text', () => {
    const text = 'This is a very long text that should be truncated';
    expect(truncateText(text, 20)).toBe('This is a very long...');
  });

  it('does not truncate short text', () => {
    expect(truncateText('Short', 20)).toBe('Short');
  });
});
```

### Custom Hooks

```typescript
// hooks/__tests__/useDebounce.test.ts
import { renderHook, act } from '@testing-library/react-native';
import { useDebounce } from '../useDebounce';

describe('useDebounce', () => {
  jest.useFakeTimers();

  it('returns initial value immediately', () => {
    const { result } = renderHook(() => useDebounce('initial', 500));
    expect(result.current).toBe('initial');
  });

  it('debounces value changes', () => {
    const { result, rerender } = renderHook(
      ({ value }) => useDebounce(value, 500),
      { initialProps: { value: 'initial' } }
    );

    rerender({ value: 'updated' });
    expect(result.current).toBe('initial');

    act(() => {
      jest.advanceTimersByTime(500);
    });

    expect(result.current).toBe('updated');
  });
});
```

---

## Component Testing

### Basic Component Test

```typescript
// components/ui/__tests__/Button.test.tsx
import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react-native';
import { Button } from '../Button';

describe('Button', () => {
  it('renders correctly', () => {
    render(<Button>Click me</Button>);
    expect(screen.getByText('Click me')).toBeOnTheScreen();
  });

  it('calls onPress when pressed', () => {
    const onPress = jest.fn();
    render(<Button onPress={onPress}>Click me</Button>);

    fireEvent.press(screen.getByText('Click me'));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('shows loading state', () => {
    render(<Button loading>Click me</Button>);
    expect(screen.getByTestId('loading-indicator')).toBeOnTheScreen();
    expect(screen.queryByText('Click me')).not.toBeOnTheScreen();
  });

  it('disables interaction when disabled', () => {
    const onPress = jest.fn();
    render(<Button onPress={onPress} disabled>Click me</Button>);

    fireEvent.press(screen.getByText('Click me'));
    expect(onPress).not.toHaveBeenCalled();
  });

  it('applies variant styles', () => {
    render(<Button variant="primary">Primary</Button>);
    const button = screen.getByTestId('button');
    expect(button).toHaveStyle({ backgroundColor: '#3B82F6' });
  });
});
```

### Screen Component Test

```typescript
// screens/__tests__/LoginScreen.test.tsx
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react-native';
import { router } from 'expo-router';
import LoginScreen from '../LoginScreen';
import { AuthProvider } from '@/services/auth/authContext';

// Mocks
jest.mock('expo-router', () => ({
  router: { replace: jest.fn() },
  Link: ({ children }: { children: React.ReactNode }) => children,
}));

const mockLogin = jest.fn();
jest.mock('@/services/auth/authContext', () => ({
  useAuth: () => ({
    login: mockLogin,
    isLoading: false,
    error: null,
    clearAuthError: jest.fn(),
  }),
  AuthProvider: ({ children }: { children: React.ReactNode }) => children,
}));

describe('LoginScreen', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders login form', () => {
    render(<LoginScreen />);

    expect(screen.getByTestId('email-input')).toBeOnTheScreen();
    expect(screen.getByTestId('password-input')).toBeOnTheScreen();
    expect(screen.getByTestId('login-button')).toBeOnTheScreen();
  });

  it('shows validation errors for empty fields', async () => {
    render(<LoginScreen />);

    fireEvent.press(screen.getByTestId('login-button'));

    await waitFor(() => {
      expect(screen.getByText(/email.*required/i)).toBeOnTheScreen();
    });
  });

  it('submits form with valid credentials', async () => {
    mockLogin.mockResolvedValue(undefined);
    render(<LoginScreen />);

    fireEvent.changeText(screen.getByTestId('email-input'), 'test@example.com');
    fireEvent.changeText(screen.getByTestId('password-input'), 'password123');
    fireEvent.press(screen.getByTestId('login-button'));

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      });
    });
  });

  it('navigates to home on successful login', async () => {
    mockLogin.mockResolvedValue(undefined);
    render(<LoginScreen />);

    fireEvent.changeText(screen.getByTestId('email-input'), 'test@example.com');
    fireEvent.changeText(screen.getByTestId('password-input'), 'password123');
    fireEvent.press(screen.getByTestId('login-button'));

    await waitFor(() => {
      expect(router.replace).toHaveBeenCalledWith('/(tabs)');
    });
  });
});
```

---

## API Mocking with MSW

```typescript
// test/mocks/handlers.ts
import { http, HttpResponse } from 'msw';

const API_URL = 'http://localhost:3000/api';

export const handlers = [
  // Auth
  http.post(`${API_URL}/auth/login`, async ({ request }) => {
    const body = await request.json();

    if (body.email === 'test@example.com') {
      return HttpResponse.json({
        user: { id: '1', email: 'test@example.com', username: 'testuser' },
        tokens: { accessToken: 'mock-access', refreshToken: 'mock-refresh' },
      });
    }

    return HttpResponse.json(
      { code: 'INVALID_CREDENTIALS', message: 'Invalid email or password' },
      { status: 401 }
    );
  }),

  // Products
  http.get(`${API_URL}/products`, () => {
    return HttpResponse.json({
      data: [
        { id: '1', name: 'Product 1', price: 9.99 },
        { id: '2', name: 'Product 2', price: 19.99 },
      ],
      page: 1,
      totalCount: 2,
      hasMore: false,
    });
  }),

  http.get(`${API_URL}/products/:id`, ({ params }) => {
    return HttpResponse.json({
      id: params.id,
      name: 'Product Detail',
      price: 29.99,
    });
  }),
];

// test/mocks/server.ts
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

export const server = setupServer(...handlers);

// jest.setup.js
import { server } from './test/mocks/server';

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

---

## Detox E2E Testing

### Configuration

```javascript
// .detoxrc.js
module.exports = {
  testRunner: {
    args: {
      $0: 'jest',
      config: 'e2e/jest.config.js',
    },
    jest: {
      setupTimeout: 120000,
    },
  },
  apps: {
    'ios.debug': {
      type: 'ios.app',
      binaryPath: 'ios/build/Build/Products/Debug-iphonesimulator/YourApp.app',
      build: 'xcodebuild -workspace ios/YourApp.xcworkspace -scheme YourApp -configuration Debug -sdk iphonesimulator -derivedDataPath ios/build',
    },
    'android.debug': {
      type: 'android.apk',
      binaryPath: 'android/app/build/outputs/apk/debug/app-debug.apk',
      build: 'cd android && ./gradlew assembleDebug assembleAndroidTest -DtestBuildType=debug',
    },
  },
  devices: {
    simulator: {
      type: 'ios.simulator',
      device: { type: 'iPhone 15' },
    },
    emulator: {
      type: 'android.emulator',
      device: { avdName: 'Pixel_4_API_30' },
    },
  },
  configurations: {
    'ios.sim.debug': {
      device: 'simulator',
      app: 'ios.debug',
    },
    'android.emu.debug': {
      device: 'emulator',
      app: 'android.debug',
    },
  },
};
```

### E2E Test Example

```typescript
// e2e/auth.test.ts
import { device, element, by, expect } from 'detox';

describe('Authentication', () => {
  beforeAll(async () => {
    await device.launchApp();
  });

  beforeEach(async () => {
    await device.reloadReactNative();
  });

  describe('Login Flow', () => {
    it('should show login screen', async () => {
      await expect(element(by.text('Welcome Back'))).toBeVisible();
      await expect(element(by.id('email-input'))).toBeVisible();
      await expect(element(by.id('password-input'))).toBeVisible();
      await expect(element(by.id('login-button'))).toBeVisible();
    });

    it('should show validation errors for empty submission', async () => {
      await element(by.id('login-button')).tap();
      await expect(element(by.text('Invalid email address'))).toBeVisible();
    });

    it('should login successfully with valid credentials', async () => {
      await element(by.id('email-input')).typeText('test@example.com');
      await element(by.id('password-input')).typeText('password123');
      await element(by.id('login-button')).tap();

      // Should navigate to home
      await expect(element(by.text('Home'))).toBeVisible();
    });

    it('should show error for invalid credentials', async () => {
      await element(by.id('email-input')).typeText('wrong@example.com');
      await element(by.id('password-input')).typeText('wrongpass');
      await element(by.id('login-button')).tap();

      await expect(element(by.text('Invalid email or password'))).toBeVisible();
    });
  });

  describe('Logout Flow', () => {
    beforeEach(async () => {
      // Login first
      await element(by.id('email-input')).typeText('test@example.com');
      await element(by.id('password-input')).typeText('password123');
      await element(by.id('login-button')).tap();
      await expect(element(by.text('Home'))).toBeVisible();
    });

    it('should logout successfully', async () => {
      await element(by.text('Profile')).tap();
      await element(by.id('logout-button')).tap();
      await expect(element(by.text('Welcome Back'))).toBeVisible();
    });
  });
});
```

---

## Maestro E2E Flows

### Flow File

```yaml
# maestro/flows/login.yaml
appId: com.yourapp.name
---
- launchApp

# Assert login screen
- assertVisible: "Welcome Back"
- assertVisible:
    id: "email-input"
- assertVisible:
    id: "password-input"

# Fill form
- tapOn:
    id: "email-input"
- inputText: "test@example.com"

- tapOn:
    id: "password-input"
- inputText: "password123"

# Submit
- tapOn:
    id: "login-button"

# Assert success navigation
- assertVisible: "Home"
```

### Running Maestro

```bash
# Run single flow
maestro test maestro/flows/login.yaml

# Run all flows
maestro test maestro/flows/

# Record for debugging
maestro record
```

---

## Test Utils

```typescript
// test/utils/renderWithProviders.tsx
import React from 'react';
import { render, RenderOptions } from '@testing-library/react-native';
import { Provider } from 'react-redux';
import { QueryClientProvider } from '@tanstack/react-query';
import { SafeAreaProvider } from 'react-native-safe-area-context';

import { store } from '@/store';
import { queryClient } from '@/services/api/queryClient';
import { AuthProvider } from '@/services/auth/authContext';

const AllTheProviders = ({ children }: { children: React.ReactNode }) => {
  return (
    <SafeAreaProvider>
      <Provider store={store}>
        <QueryClientProvider client={queryClient}>
          <AuthProvider>{children}</AuthProvider>
        </QueryClientProvider>
      </Provider>
    </SafeAreaProvider>
  );
};

const customRender = (ui: React.ReactElement, options?: RenderOptions) =>
  render(ui, { wrapper: AllTheProviders, ...options });

export * from '@testing-library/react-native';
export { customRender as render };
```

---

## CI Configuration

```yaml
# .github/workflows/test.yml
name: Test

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'

      - run: npm ci
      - run: npm run lint
      - run: npm run test -- --coverage

      - uses: codecov/codecov-action@v4
        with:
          files: ./coverage/lcov.info

  e2e-ios:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'

      - run: npm ci
      - run: brew tap wix/brew && brew install applesimutils
      - run: npx detox build --configuration ios.sim.debug
      - run: npx detox test --configuration ios.sim.debug
```

---

## Context7 Integration

When uncertain about testing APIs or patterns, query Context7:

```
1. Use resolve-library-id to find: "jest", "testing-library", "detox"
2. Query specific topics:
   - "Jest mock functions and timers"
   - "React Native Testing Library queries"
   - "Detox device actions and matchers"
   - "MSW request handlers React Native"
```

---

## Related Skills

- `rn-fundamentals` - Component patterns
- `rn-navigation` - Navigation testing
- `rn-api-integration` - API mocking
- `rn-auth-integration` - Auth flow testing
