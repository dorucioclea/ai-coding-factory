# React Frontend Template

A production-ready React frontend template designed to work seamlessly with the .NET Clean Architecture backend. Built with Next.js 14+, TypeScript, and modern best practices.

## Features

- **Next.js 14** with App Router and React Server Components
- **TypeScript** with strict configuration
- **shadcn/ui** components with Tailwind CSS
- **TanStack Query** for server state management
- **Zustand** for client state management
- **JWT Authentication** matching backend implementation
- **React Hook Form** with Zod validation
- **TanStack Table** for data tables
- **Dark mode** with next-themes
- **Internationalization** with next-intl (EN, ES ready)
- **Docker** ready with multi-stage builds
- **Vitest** for unit testing (80%+ coverage target)
- **Playwright** for E2E testing

## Quick Start

### Prerequisites

- Node.js 20+
- npm, yarn, or pnpm
- Docker (optional, for containerized development)

### Installation

```bash
# Clone or copy this template
cp -r templates/react-frontend-template my-frontend

cd my-frontend

# Install dependencies
npm install

# Copy environment file
cp .env.example .env.local

# Start development server
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) to see your application.

### Environment Variables

```bash
# Application
NEXT_PUBLIC_APP_NAME=ProjectName
NEXT_PUBLIC_APP_URL=http://localhost:3000

# API Configuration
NEXT_PUBLIC_API_URL=http://localhost:5000/api

# Authentication
JWT_SECRET=your-256-bit-secret-key-here-min-32-chars
```

## Project Structure

```
src/
├── app/                    # Next.js App Router pages
│   ├── auth/              # Authentication pages
│   │   ├── login/
│   │   ├── register/
│   │   └── forgot-password/
│   ├── dashboard/         # Protected dashboard
│   └── settings/          # User settings
├── components/
│   ├── ui/                # shadcn/ui components
│   ├── layout/            # Layout components (Header, Sidebar)
│   ├── forms/             # Form components
│   └── tables/            # Data table components
├── hooks/                 # Custom React hooks
│   └── use-auth.ts        # Authentication hook
├── lib/
│   ├── api-client.ts      # API client with auth
│   ├── auth.ts            # Auth service
│   ├── query-client.ts    # TanStack Query setup
│   ├── utils.ts           # Utility functions
│   └── validations/       # Zod schemas
├── stores/                # Zustand stores
│   ├── ui-store.ts        # UI state
│   └── settings-store.ts  # User preferences
├── types/                 # TypeScript types
│   ├── auth.ts            # Auth types
│   └── api.ts             # API types
├── i18n/                  # Internationalization
│   ├── request.ts         # next-intl config
│   └── messages/          # Translation files
└── styles/
    └── globals.css        # Global styles & CSS variables
```

## Integration with Backend

This template is designed to work with the Clean Architecture .NET backend. The authentication system matches the backend's JWT implementation:

### API Client Configuration

The `src/lib/api-client.ts` handles:
- Automatic token injection in headers
- Token refresh on 401 responses
- RFC 7807 Problem Details error parsing
- Cookie-based token storage (SSR compatible)

### Auth Types Mapping

| Frontend (TypeScript)    | Backend (.NET)            |
|-------------------------|---------------------------|
| `User`                  | User entity               |
| `AuthTokens`            | JWT response              |
| `LoginRequest`          | LoginCommand              |
| `RegisterRequest`       | RegisterCommand           |
| `JwtPayload`            | JWT claims (sub, email, roles) |

### Backend Endpoints Expected

```
POST /api/auth/login        - User login
POST /api/auth/register     - User registration
POST /api/auth/logout       - User logout
POST /api/auth/refresh      - Token refresh
GET  /api/auth/me           - Current user profile
POST /api/auth/forgot-password
POST /api/auth/reset-password
POST /api/auth/change-password
```

## Available Scripts

```bash
# Development
npm run dev              # Start development server
npm run build            # Build for production
npm run start            # Start production server

# Code Quality
npm run lint             # Run ESLint
npm run lint:fix         # Fix ESLint issues
npm run type-check       # TypeScript checking
npm run format           # Format with Prettier
npm run format:check     # Check formatting

# Testing
npm run test             # Run unit tests
npm run test:ui          # Run tests with UI
npm run test:coverage    # Run tests with coverage
npm run test:e2e         # Run E2E tests
npm run test:e2e:ui      # Run E2E tests with UI
```

## Docker

### Development

```bash
# Start with Docker Compose (includes backend, db, redis)
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up

# With pgAdmin for database management
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --profile tools up
```

### Production

```bash
# Build image
docker build -t my-frontend:latest \
  --build-arg NEXT_PUBLIC_API_URL=https://api.example.com \
  --build-arg NEXT_PUBLIC_APP_URL=https://app.example.com \
  .

# Run container
docker run -p 3000:3000 my-frontend:latest
```

## Adding UI Components

This template uses shadcn/ui. To add more components:

```bash
# Install shadcn/ui CLI (one-time)
npx shadcn@latest init

# Add components
npx shadcn@latest add accordion
npx shadcn@latest add alert-dialog
npx shadcn@latest add calendar
```

## Authentication Flow

```typescript
import { useAuth } from '@/hooks/use-auth';

function MyComponent() {
  const {
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
    hasRole
  } = useAuth();

  // Check roles
  if (hasRole('Admin')) {
    // Admin-only content
  }

  // Login
  await login({ email: 'user@example.com', password: '...' });

  // Logout
  logout();
}
```

## Form Handling

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const schema = z.object({
  name: z.string().min(1, 'Required'),
  email: z.string().email('Invalid email'),
});

function MyForm() {
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(schema),
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <FormField control={control} name="name" label="Name" />
      <FormField control={control} name="email" label="Email" type="email" />
      <Button type="submit">Submit</Button>
    </form>
  );
}
```

## API Queries with TanStack Query

```typescript
import { useQuery, useMutation } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';

// Query
const { data, isLoading } = useQuery({
  queryKey: queryKeys.users.list({ page: 1 }),
  queryFn: () => apiClient.get('/users', { params: { page: 1 } }),
});

// Mutation
const mutation = useMutation({
  mutationFn: (data) => apiClient.post('/users', data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: queryKeys.users.all });
  },
});
```

## Testing Strategy

### Unit Tests (Vitest)

```bash
npm run test           # Run all tests
npm run test:coverage  # With coverage report
```

Coverage targets: 80% minimum for branches, functions, lines, statements.

### E2E Tests (Playwright)

```bash
npm run test:e2e       # Run E2E tests
npm run test:e2e:ui    # With interactive UI
```

## Customization

### Theming

Modify CSS variables in `src/styles/globals.css`:

```css
:root {
  --primary: 221.2 83.2% 53.3%;
  --secondary: 210 40% 96.1%;
  /* ... */
}
```

### Adding Languages

1. Create translation file: `src/i18n/messages/de.json`
2. Add locale to `src/i18n/request.ts`

## Best Practices

1. **Use Server Components** where possible for better performance
2. **Co-locate related files** - keep components, hooks, and tests together
3. **Type everything** - leverage TypeScript for safety
4. **Validate at boundaries** - use Zod schemas for API and form data
5. **Handle errors gracefully** - use error boundaries and proper error states
6. **Test user journeys** - E2E tests for critical flows

## License

MIT
