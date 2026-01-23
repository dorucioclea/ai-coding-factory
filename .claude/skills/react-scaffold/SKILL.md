---
name: react-scaffold
description: Use when creating React/Next.js frontend projects, adding React components, or implementing frontend features. References the react-frontend-template and best practices.
---

# React Scaffold

## Overview

Guide for scaffolding and building React/Next.js frontends using the `react-frontend-template`. Ensures consistent patterns, proper auth integration, and best practices.

## When to Use

- Creating new React/Next.js frontend project
- Adding new pages or components
- Implementing auth features
- Connecting frontend to .NET backend
- Building forms with validation
- Adding data tables

## Template Reference

**Location:** `templates/react-frontend-template/`

**Tech Stack:**
- Next.js 14+ (App Router)
- TypeScript (strict)
- shadcn/ui + Tailwind CSS
- TanStack Query (server state)
- Zustand (client state)
- react-hook-form + Zod
- next-themes (dark mode)
- next-intl (i18n)

## Scaffolding New Project

```bash
/scaffold {ProjectName} react-frontend
```

This creates `projects/{ProjectName}-frontend/` with full structure.

## Key Files to Know

| Purpose | File | Notes |
|---------|------|-------|
| API Client | `src/lib/api-client.ts` | Auto auth headers, token refresh |
| Auth Service | `src/lib/auth.ts` | Login, logout, JWT handling |
| Auth Hook | `src/hooks/use-auth.ts` | `useAuth()` for components |
| Query Keys | `src/lib/query-client.ts` | Type-safe query key factory |
| Form Validation | `src/lib/validations/` | Zod schemas |
| UI Components | `src/components/ui/` | shadcn/ui primitives |
| Layout | `src/components/layout/` | Header, Sidebar, Providers |

## Adding New Features

### 1. New Page

Create in `src/app/{route}/page.tsx`:

```tsx
import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Page Title',
};

export default function MyPage() {
  return (
    <div>
      <h1>My Page</h1>
    </div>
  );
}
```

### 2. Protected Page

Use `useRequireAuth` hook:

```tsx
'use client';

import { useRequireAuth } from '@/hooks/use-auth';

export default function ProtectedPage() {
  const { user, isLoading } = useRequireAuth();

  if (isLoading) return <LoadingSkeleton />;

  return <div>Welcome, {user?.firstName}</div>;
}
```

### 3. New API Query

```tsx
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';

// In component
const { data, isLoading } = useQuery({
  queryKey: queryKeys.entity('fishingSpots').list({ page: 1 }),
  queryFn: () => apiClient.get('/fishing-spots', { params: { page: 1 } }),
});
```

### 4. New Mutation

```tsx
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';

const queryClient = useQueryClient();

const createSpot = useMutation({
  mutationFn: (data: CreateSpotDto) =>
    apiClient.post('/fishing-spots', data),
  onSuccess: () => {
    queryClient.invalidateQueries({
      queryKey: queryKeys.entity('fishingSpots').all
    });
  },
});
```

### 5. New Form with Validation

```tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { FormField } from '@/components/forms';
import { Button } from '@/components/ui';

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  location: z.string().min(1, 'Location is required'),
});

type FormData = z.infer<typeof schema>;

export function SpotForm() {
  const { control, handleSubmit } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: FormData) => {
    // Call mutation
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <FormField control={control} name="name" label="Name" required />
      <FormField control={control} name="location" label="Location" required />
      <Button type="submit">Create</Button>
    </form>
  );
}
```

### 6. New shadcn/ui Component

If component doesn't exist:

```bash
npx shadcn@latest add {component-name}
```

Common additions: `dialog`, `tabs`, `calendar`, `combobox`

## Backend Integration

### Match JWT Configuration

Frontend `.env.local`:
```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
JWT_SECRET=same-as-backend-min-32-chars
```

Backend `appsettings.json`:
```json
{
  "Jwt": {
    "Secret": "same-as-backend-min-32-chars",
    "Issuer": "ProjectName",
    "Audience": "ProjectName"
  }
}
```

### CORS Configuration

Backend must allow frontend origin:
```csharp
// In Program.cs or extension
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### Auth Flow

1. Frontend calls `POST /api/auth/login`
2. Backend returns JWT tokens
3. Frontend stores in cookies via `tokenStorage`
4. Subsequent requests include `Authorization: Bearer {token}`
5. On 401, frontend attempts refresh via `POST /api/auth/refresh`

## Patterns to Follow

### State Management

| State Type | Solution |
|------------|----------|
| Server data | TanStack Query |
| UI state (sidebar, modals) | Zustand `useUIStore` |
| User preferences | Zustand `useSettingsStore` |
| Form state | react-hook-form |
| Auth state | `useAuth` hook (wraps Query) |

### Component Organization

```
src/components/
├── ui/           # Generic, reusable (shadcn)
├── layout/       # App shell (Header, Sidebar)
├── forms/        # Form components with validation
├── tables/       # Data display
└── features/     # Feature-specific components
    └── fishing-spots/
        ├── SpotCard.tsx
        ├── SpotForm.tsx
        └── SpotList.tsx
```

### File Naming

- Components: `PascalCase.tsx`
- Hooks: `use-kebab-case.ts`
- Utils: `kebab-case.ts`
- Types: `kebab-case.ts`

## Testing

### Unit Tests (Vitest)

```tsx
// src/__tests__/components/spot-card.test.tsx
import { render, screen } from '@testing-library/react';
import { SpotCard } from '@/components/features/fishing-spots/SpotCard';

describe('SpotCard', () => {
  it('displays spot name', () => {
    render(<SpotCard name="Lake Spot" />);
    expect(screen.getByText('Lake Spot')).toBeInTheDocument();
  });
});
```

### E2E Tests (Playwright)

```tsx
// e2e/fishing-spots.spec.ts
import { test, expect } from '@playwright/test';

test('can view fishing spots', async ({ page }) => {
  await page.goto('/dashboard/spots');
  await expect(page.getByRole('heading', { name: /spots/i })).toBeVisible();
});
```

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Hardcoding API URL | Use `NEXT_PUBLIC_API_URL` |
| Not handling loading states | Always check `isLoading` |
| Missing error boundaries | Add ErrorBoundary components |
| Client component without `'use client'` | Add directive for hooks |
| Forgetting auth in API calls | `apiClient` handles automatically |

## Quick Reference

```bash
# Development
npm run dev

# Type check
npm run type-check

# Tests
npm run test
npm run test:e2e

# Build
npm run build
```
