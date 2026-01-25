<!-- React Native navigation patterns - Expo Router, React Navigation, deep linking, tab/stack/drawer navigation, authentication flows -->


# React Native Navigation

## Overview

Comprehensive navigation patterns for React Native applications using Expo Router (recommended) and React Navigation. Covers stack, tab, drawer navigation, deep linking, and authentication flows.

## When to Use

- Setting up app navigation structure
- Implementing authentication flows with protected routes
- Configuring deep linking
- Building complex navigation hierarchies
- Migrating from React Navigation to Expo Router

## When NOT to Use

- Basic component development (use `rn-fundamentals`)
- State management (use `rn-state-management`)
- Screen transitions/animations (use `rn-animations`)

---

## Expo Router (Recommended)

### File-Based Routing Structure

```
app/
├── _layout.tsx              # Root layout (providers, auth)
├── index.tsx                # Home screen (/)
├── (auth)/                  # Auth group (unauthenticated)
│   ├── _layout.tsx          # Auth layout
│   ├── login.tsx            # /login
│   ├── register.tsx         # /register
│   └── forgot-password.tsx  # /forgot-password
├── (tabs)/                  # Main app tabs (authenticated)
│   ├── _layout.tsx          # Tab navigator layout
│   ├── index.tsx            # Home tab
│   ├── search.tsx           # Search tab
│   └── profile.tsx          # Profile tab
├── (modals)/                # Modal screens
│   ├── _layout.tsx          # Modal layout
│   └── settings.tsx         # Settings modal
├── details/
│   └── [id].tsx             # Dynamic route /details/:id
└── +not-found.tsx           # 404 screen
```

### Root Layout with Providers

```typescript
// app/_layout.tsx
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { Provider } from 'react-redux';
import { QueryClientProvider } from '@tanstack/react-query';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import * as Sentry from '@sentry/react-native';

import { store } from '@/store';
import { queryClient } from '@/services/api/queryClient';
import { AuthProvider } from '@/services/auth/authContext';
import { ThemeProvider } from '@/theme';
import { initSentry } from '@/observability/sentry.config';

// Initialize Sentry
initSentry();

export default function RootLayout() {
  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <SafeAreaProvider>
        <Provider store={store}>
          <QueryClientProvider client={queryClient}>
            <ThemeProvider>
              <AuthProvider>
                <StatusBar style="auto" />
                <Stack screenOptions={{ headerShown: false }}>
                  <Stack.Screen name="(auth)" />
                  <Stack.Screen name="(tabs)" />
                  <Stack.Screen
                    name="(modals)"
                    options={{ presentation: 'modal' }}
                  />
                </Stack>
              </AuthProvider>
            </ThemeProvider>
          </QueryClientProvider>
        </Provider>
      </SafeAreaProvider>
    </GestureHandlerRootView>
  );
}
```

### Tab Navigator

```typescript
// app/(tabs)/_layout.tsx
import { Tabs } from 'expo-router';
import { Home, Search, User } from 'lucide-react-native';
import { useColorScheme } from '@/hooks/useColorScheme';
import { colors } from '@/theme';

export default function TabLayout() {
  const { isDark } = useColorScheme();

  const tabBarStyle = {
    backgroundColor: isDark ? colors.neutral[900] : colors.neutral[50],
    borderTopColor: isDark ? colors.neutral[800] : colors.neutral[200],
  };

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle,
        tabBarActiveTintColor: colors.primary[500],
        tabBarInactiveTintColor: colors.neutral[500],
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'Home',
          tabBarIcon: ({ color, size }) => <Home color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="search"
        options={{
          title: 'Search',
          tabBarIcon: ({ color, size }) => <Search color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Profile',
          tabBarIcon: ({ color, size }) => <User color={color} size={size} />,
        }}
      />
    </Tabs>
  );
}
```

### Protected Routes with Auth

```typescript
// app/(tabs)/_layout.tsx
import { Redirect, Tabs } from 'expo-router';
import { useAuth } from '@/services/auth/useAuth';
import { LoadingScreen } from '@/components/shared/LoadingScreen';

export default function TabLayout() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  if (!isAuthenticated) {
    return <Redirect href="/login" />;
  }

  return (
    <Tabs>
      {/* Tab screens */}
    </Tabs>
  );
}

// app/(auth)/_layout.tsx
import { Redirect, Stack } from 'expo-router';
import { useAuth } from '@/services/auth/useAuth';

export default function AuthLayout() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  // Already authenticated, redirect to main app
  if (isAuthenticated) {
    return <Redirect href="/(tabs)" />;
  }

  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="login" />
      <Stack.Screen name="register" />
      <Stack.Screen name="forgot-password" />
    </Stack>
  );
}
```

### Dynamic Routes

```typescript
// app/details/[id].tsx
import { useLocalSearchParams, Stack, router } from 'expo-router';
import { View, Text, Pressable } from 'react-native';

export default function DetailsScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();

  return (
    <View style={{ flex: 1 }}>
      <Stack.Screen
        options={{
          title: `Details ${id}`,
          headerShown: true,
        }}
      />
      <Text>Viewing item: {id}</Text>
      <Pressable onPress={() => router.back()}>
        <Text>Go Back</Text>
      </Pressable>
    </View>
  );
}

// Navigation to dynamic route
import { router } from 'expo-router';

// Push navigation
router.push(`/details/${itemId}`);

// Replace (no back)
router.replace(`/details/${itemId}`);

// With params object
router.push({
  pathname: '/details/[id]',
  params: { id: itemId },
});
```

### Deep Linking Configuration

```typescript
// app.json
{
  "expo": {
    "scheme": "myapp",
    "web": {
      "bundler": "metro"
    },
    "plugins": [
      [
        "expo-router",
        {
          "origin": "https://myapp.com"
        }
      ]
    ]
  }
}

// Handling deep links in app
// Links automatically work with Expo Router:
// myapp://details/123 -> /details/123
// https://myapp.com/details/123 -> /details/123
```

---

## Navigation Patterns

### Stack with Header

```typescript
// app/profile/_layout.tsx
import { Stack } from 'expo-router';

export default function ProfileLayout() {
  return (
    <Stack
      screenOptions={{
        headerStyle: {
          backgroundColor: colors.neutral[50],
        },
        headerTintColor: colors.neutral[900],
        headerTitleStyle: {
          fontWeight: '600',
        },
        headerShadowVisible: false,
      }}
    >
      <Stack.Screen
        name="index"
        options={{ title: 'Profile' }}
      />
      <Stack.Screen
        name="edit"
        options={{ title: 'Edit Profile' }}
      />
      <Stack.Screen
        name="settings"
        options={{ title: 'Settings' }}
      />
    </Stack>
  );
}
```

### Modal Presentation

```typescript
// app/(modals)/_layout.tsx
import { Stack } from 'expo-router';

export default function ModalsLayout() {
  return (
    <Stack
      screenOptions={{
        presentation: 'modal',
        headerShown: true,
        gestureEnabled: true,
        gestureDirection: 'vertical',
      }}
    >
      <Stack.Screen
        name="settings"
        options={{
          title: 'Settings',
          headerLeft: () => null, // Remove back button
        }}
      />
    </Stack>
  );
}

// Opening modal
import { router } from 'expo-router';

router.push('/(modals)/settings');

// Dismissing modal
router.back();
// or
router.dismiss();
```

### Drawer Navigation

```typescript
// app/(drawer)/_layout.tsx
import { Drawer } from 'expo-router/drawer';
import { DrawerContentScrollView, DrawerItem } from '@react-navigation/drawer';

function CustomDrawerContent(props) {
  return (
    <DrawerContentScrollView {...props}>
      <DrawerItem
        label="Home"
        onPress={() => props.navigation.navigate('index')}
      />
      <DrawerItem
        label="Settings"
        onPress={() => props.navigation.navigate('settings')}
      />
    </DrawerContentScrollView>
  );
}

export default function DrawerLayout() {
  return (
    <Drawer
      drawerContent={CustomDrawerContent}
      screenOptions={{
        headerShown: true,
        drawerType: 'front',
        drawerStyle: { width: 280 },
      }}
    >
      <Drawer.Screen name="index" options={{ title: 'Home' }} />
      <Drawer.Screen name="settings" options={{ title: 'Settings' }} />
    </Drawer>
  );
}
```

---

## Navigation Hooks & Utilities

### Programmatic Navigation

```typescript
import { router, useRouter, usePathname, useSegments } from 'expo-router';

function NavigationExample() {
  const routerHook = useRouter();
  const pathname = usePathname();        // Current path: "/details/123"
  const segments = useSegments();        // ["details", "123"]

  // Navigation methods
  const navigate = () => {
    // Push (adds to stack)
    router.push('/profile');

    // Replace (no back navigation)
    router.replace('/home');

    // Back
    router.back();

    // Dismiss modal
    router.dismiss();

    // Navigate with params
    router.push({
      pathname: '/search',
      params: { query: 'test' },
    });

    // Can go back?
    if (router.canGoBack()) {
      router.back();
    }
  };

  return <View />;
}
```

### Navigation State Persistence

```typescript
// app/_layout.tsx
import { useNavigationContainerRef, useRootNavigationState } from 'expo-router';
import AsyncStorage from '@react-native-async-storage/async-storage';

const NAVIGATION_STATE_KEY = 'NAVIGATION_STATE';

export default function RootLayout() {
  const rootNavigationState = useRootNavigationState();

  // Save navigation state
  useEffect(() => {
    if (rootNavigationState?.key) {
      AsyncStorage.setItem(
        NAVIGATION_STATE_KEY,
        JSON.stringify(rootNavigationState)
      );
    }
  }, [rootNavigationState]);

  return <Stack />;
}
```

---

## Observability Integration

### Navigation Tracking with Sentry

```typescript
// observability/navigationTracking.ts
import * as Sentry from '@sentry/react-native';
import { useNavigationContainerRef } from 'expo-router';
import { useEffect } from 'react';

const routingInstrumentation = new Sentry.ReactNavigationInstrumentation();

export function useNavigationTracking() {
  const navigationRef = useNavigationContainerRef();

  useEffect(() => {
    if (navigationRef) {
      routingInstrumentation.registerNavigationContainer(navigationRef);
    }
  }, [navigationRef]);
}

// Export for Sentry init
export { routingInstrumentation };

// In Sentry.init()
Sentry.init({
  integrations: [
    new Sentry.ReactNativeTracing({
      routingInstrumentation,
    }),
  ],
});
```

### Breadcrumbs for Navigation

```typescript
// hooks/useNavigationBreadcrumbs.ts
import * as Sentry from '@sentry/react-native';
import { usePathname } from 'expo-router';
import { useEffect, useRef } from 'react';

export function useNavigationBreadcrumbs() {
  const pathname = usePathname();
  const previousPath = useRef<string | null>(null);

  useEffect(() => {
    if (previousPath.current !== pathname) {
      Sentry.addBreadcrumb({
        category: 'navigation',
        message: `Navigated to ${pathname}`,
        level: 'info',
        data: {
          from: previousPath.current,
          to: pathname,
        },
      });
      previousPath.current = pathname;
    }
  }, [pathname]);
}
```

---

## Best Practices

### URL Pattern Design

```
/                           # Home
/login                      # Login screen
/register                   # Registration
/profile                    # User profile
/profile/edit               # Edit profile
/items                      # Item list
/items/[id]                 # Item details
/items/[id]/comments        # Item comments
/settings                   # Settings (modal)
/search?q=query             # Search with query params
```

### Navigation Testing

```typescript
// __tests__/navigation.test.tsx
import { render, fireEvent } from '@testing-library/react-native';
import { router } from 'expo-router';

jest.mock('expo-router', () => ({
  router: {
    push: jest.fn(),
    replace: jest.fn(),
    back: jest.fn(),
  },
  useLocalSearchParams: () => ({ id: '123' }),
}));

describe('Navigation', () => {
  it('navigates to details on item press', () => {
    const { getByTestId } = render(<ItemCard id="123" />);

    fireEvent.press(getByTestId('item-card'));

    expect(router.push).toHaveBeenCalledWith('/details/123');
  });
});
```

---

## Context7 Integration

Query Context7 for Expo Router updates:

```
1. resolve-library-id: "expo-router"
2. Query: "Expo Router authentication protected routes"
3. Query: "Expo Router deep linking configuration"
```

---

## Common Mistakes

| Mistake | Correct Approach |
|---------|------------------|
| Using React Navigation in new Expo projects | Use Expo Router (file-based) |
| Not wrapping in GestureHandlerRootView | Required for gestures |
| Missing SafeAreaProvider | Wrap at root level |
| Auth checks in every screen | Use layout-level redirects |
| Hardcoded navigation paths | Use typed route constants |

---

## Related Skills

- `rn-fundamentals` - Core React Native patterns
- `rn-auth-integration` - Authentication flows
- `rn-observability-setup` - Navigation tracking
- `rn-animations` - Screen transitions
