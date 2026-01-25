# ProjectName - React Native/Expo App

A React Native mobile application built with Expo SDK 52+, featuring:

- **Expo Router v4** - File-based routing
- **Redux Toolkit** - Client state management with persistence
- **TanStack Query** - Server state management
- **Sentry** - Full observability (errors, performance, breadcrumbs)
- **JWT Authentication** - Secure token storage with expo-secure-store
- **Minimalist Modern Design System** - Consistent design tokens

## Prerequisites

- Node.js 20+
- npm or yarn
- Expo CLI (`npm install -g expo-cli`)
- EAS CLI (`npm install -g eas-cli`)
- iOS Simulator (macOS) or Android Emulator

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment

```bash
cp .env.example .env
```

Edit `.env` with your values:
```
API_URL=http://localhost:5000/api
SENTRY_DSN=your-sentry-dsn
```

### 3. Start Development Server

```bash
# Start Expo development server
npm start

# Or start with specific platform
npm run dev:ios
npm run dev:android
```

## Project Structure

```
├── app/                    # Expo Router screens
│   ├── _layout.tsx         # Root layout (providers)
│   ├── index.tsx           # Entry redirect
│   ├── (auth)/             # Auth flow screens
│   │   ├── login.tsx
│   │   ├── register.tsx
│   │   └── forgot-password.tsx
│   └── (app)/              # Main app (protected)
│       └── (tabs)/         # Tab navigator
│           ├── index.tsx   # Home
│           ├── explore.tsx # Search
│           └── profile.tsx # Profile
├── components/
│   └── ui/                 # Design system components
├── hooks/                  # Custom hooks
├── services/
│   ├── api/                # API client, query client
│   └── auth/               # Auth service, secure storage
├── slices/                 # Redux slices
├── store/                  # Redux store configuration
├── theme/                  # Design tokens
├── types/                  # TypeScript types
├── observability/          # Sentry integration
└── utils/                  # Utilities
```

## Available Scripts

```bash
# Development
npm start           # Start Expo server
npm run dev:ios     # Start on iOS simulator
npm run dev:android # Start on Android emulator

# Quality
npm run lint        # Run ESLint
npm run type-check  # Run TypeScript check
npm test            # Run Jest tests
npm run test:coverage # Run tests with coverage

# Build
npm run build:dev   # EAS development build
npm run build:preview # EAS preview build
npm run build:prod  # EAS production build
```

## Architecture

### State Management

- **Client State** (Redux Toolkit): Auth state, settings, UI state
- **Server State** (TanStack Query): API data, caching, mutations

### Authentication Flow

1. JWT tokens stored in expo-secure-store
2. Automatic token refresh on 401
3. Protected routes via Expo Router layouts

### Observability

- Error boundaries at root and screen levels
- Navigation breadcrumbs
- Screen load performance tracking
- API request timing

## Connecting to Backend

This template is designed to work with the clean-architecture-solution backend:

1. Start the backend: `cd projects/YourBackend && dotnet run`
2. Set `API_URL=http://localhost:5000/api` in `.env`
3. Ensure CORS is configured for your mobile development URL

## Building for Production

### Configure EAS

```bash
eas login
eas build:configure
```

### Set Secrets

```bash
eas secret:create --name SENTRY_DSN --value "your-dsn"
eas secret:create --name API_URL --value "https://api.production.com"
```

### Build

```bash
# Preview (TestFlight/Internal)
eas build --profile preview --platform all

# Production
eas build --profile production --platform all
```

### Submit to Stores

```bash
eas submit --platform ios
eas submit --platform android
```

## Contributing

1. Create a feature branch
2. Make changes with tests
3. Run `npm run lint && npm run type-check && npm test`
4. Create a pull request

## License

MIT
