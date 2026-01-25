# CLAUDE.md - React Native Template Instructions

## Project Overview

This is a React Native/Expo mobile application template with:
- Expo SDK 52+ with New Architecture
- Expo Router v4 (file-based routing)
- Redux Toolkit + TanStack Query (state management)
- Sentry (full observability)
- JWT Authentication with expo-secure-store
- Minimalist Modern Design System

## Key Patterns

### File-Based Routing (Expo Router)

```
app/
├── _layout.tsx         # Root layout - providers, error boundary
├── index.tsx           # Entry - redirects based on auth
├── (auth)/             # Auth flow (unauthenticated)
│   ├── _layout.tsx     # Auth layout
│   ├── login.tsx
│   └── register.tsx
└── (app)/              # Main app (authenticated)
    ├── _layout.tsx     # Auth guard
    └── (tabs)/         # Tab navigator
```

### State Management

- **Client State (Redux)**: Auth, settings, UI state
- **Server State (TanStack Query)**: API data with caching

```typescript
// Redux for client state
const user = useAppSelector(selectUser);
dispatch(login(credentials));

// TanStack Query for server state
const { data, isLoading } = useQuery({ queryKey: ['users'], queryFn: fetchUsers });
```

### Authentication

1. Tokens stored in `expo-secure-store` (authStorage.ts)
2. API client automatically attaches tokens
3. 401 responses trigger token refresh
4. Protected routes in `(app)/_layout.tsx`

### Observability

- `useScreenTransaction(screenName)` - Track screen load
- `useBreadcrumbs()` - Track user actions
- `ScreenErrorBoundary` - Catch screen-level errors

### Design System

Use theme tokens, not hardcoded values:
```typescript
const { colors, spacing, typography } = useTheme();

<View style={{ backgroundColor: colors.background.primary, padding: spacing.md }}>
  <Text style={{ color: colors.text.primary, fontSize: typography.fontSize.md }}>
```

## Commands

When working in this project:

- `/add-mobile-screen` - Add new Expo Router screen
- `/add-mobile-service` - Add API service with TanStack Query
- `/add-mobile-auth` - Add authentication (already included)
- `/add-mobile-observability` - Add Sentry (already included)
- `/mobile-e2e` - Generate E2E tests (Maestro)
- `/mobile-deploy` - Build with EAS

## Common Tasks

### Add a New Screen

1. Create file in `app/` following route structure
2. Wrap with `ScreenErrorBoundary`
3. Use `useScreenTransaction` for performance tracking
4. Use theme tokens for styling

### Add API Integration

1. Create types in `types/shared/`
2. Create service in `services/api/`
3. Create hooks in `hooks/api/` using TanStack Query
4. Use optimistic updates for better UX

### Style Components

Always use theme tokens:
```typescript
const { colors, spacing, typography, radius, shadows } = useTheme();
```

## Testing

```bash
npm test                    # Run Jest tests
npm run test:coverage       # With coverage
maestro test e2e/flows/     # E2E tests
```

## Build & Deploy

```bash
eas build --profile preview --platform all  # Preview build
eas build --profile production --platform all  # Production
eas submit --platform ios   # Submit to App Store
```

## Environment Variables

Required in `.env`:
- `API_URL` - Backend API URL
- `SENTRY_DSN` - Sentry DSN for observability

Set secrets for production:
```bash
eas secret:create --name API_URL --value "https://api.example.com"
```
