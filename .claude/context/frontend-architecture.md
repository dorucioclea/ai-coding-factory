# Frontend Architecture Quick Reference

## Project Structure

### Next.js App Router (Recommended)
```
src/
├── app/                    # App Router pages
│   ├── layout.tsx         # Root layout
│   ├── page.tsx           # Home page
│   ├── loading.tsx        # Loading UI
│   ├── error.tsx          # Error UI
│   ├── not-found.tsx      # 404 page
│   ├── (auth)/            # Route group (no URL impact)
│   │   ├── login/
│   │   └── register/
│   └── dashboard/
│       ├── layout.tsx     # Nested layout
│       └── page.tsx
├── components/
│   ├── ui/                # Reusable UI components
│   │   ├── Button/
│   │   ├── Card/
│   │   └── Input/
│   ├── features/          # Feature-specific components
│   │   ├── auth/
│   │   └── dashboard/
│   └── layouts/           # Layout components
├── hooks/                 # Custom React hooks
├── lib/                   # Utilities & configurations
│   ├── api/              # API client
│   ├── utils/            # Helper functions
│   └── validations/      # Zod schemas
├── styles/               # Global styles
├── types/                # TypeScript types
└── providers/            # Context providers
```

### Feature-Based Organization
```
src/
├── features/
│   ├── auth/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── api/
│   │   ├── types/
│   │   └── index.ts
│   ├── dashboard/
│   └── settings/
└── shared/
    ├── components/
    ├── hooks/
    └── utils/
```

## Component Architecture

### Atomic Design
```
atoms/       → Button, Input, Label
molecules/   → FormField, SearchBox
organisms/   → Header, Sidebar, Form
templates/   → DashboardLayout
pages/       → Dashboard, Settings
```

### Component File Structure
```
Button/
├── Button.tsx           # Component
├── Button.test.tsx      # Tests
├── Button.module.css    # Styles
├── Button.stories.tsx   # Storybook (optional)
├── types.ts             # Types (if complex)
└── index.ts             # Barrel export
```

## State Management Layers

### Layer 1: Local State
```tsx
// UI state: form inputs, toggles, animations
const [isOpen, setIsOpen] = useState(false);
```

### Layer 2: Shared State (Context)
```tsx
// Theme, auth, user preferences
const { user } = useAuth();
const { theme } = useTheme();
```

### Layer 3: Server State (TanStack Query)
```tsx
// API data with caching, sync, background updates
const { data, isLoading } = useQuery({
  queryKey: ['users'],
  queryFn: fetchUsers,
});
```

### Layer 4: Global State (Zustand/Redux)
```tsx
// Complex client state spanning many components
const { items, addItem } = useCartStore();
```

## Data Fetching Patterns

### Server Components (Next.js 14+)
```tsx
// app/users/page.tsx - Runs on server
async function UsersPage() {
  const users = await fetchUsers(); // Direct fetch
  return <UserList users={users} />;
}
```

### Client Components + TanStack Query
```tsx
'use client';

function UserList() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['users'],
    queryFn: () => api.get('/users'),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  if (isLoading) return <Skeleton />;
  if (error) return <Error />;
  return <List items={data} />;
}
```

### Mutations
```tsx
const mutation = useMutation({
  mutationFn: (user: User) => api.post('/users', user),
  onSuccess: () => {
    queryClient.invalidateQueries(['users']);
    toast.success('User created');
  },
});
```

## Styling Approaches

### CSS Modules (Recommended)
```tsx
import styles from './Button.module.css';

<button className={styles.primary}>Click</button>
```

### Tailwind CSS
```tsx
<button className="bg-blue-500 hover:bg-blue-700 px-4 py-2">
  Click
</button>
```

### CSS-in-JS (styled-components)
```tsx
const Button = styled.button`
  background: ${props => props.primary ? 'blue' : 'gray'};
`;
```

## Performance Optimization

### Code Splitting
```tsx
// Dynamic imports
const HeavyComponent = dynamic(() => import('./HeavyComponent'), {
  loading: () => <Skeleton />,
});

// Route-based splitting (automatic in Next.js)
```

### Image Optimization
```tsx
import Image from 'next/image';

<Image
  src="/hero.jpg"
  width={800}
  height={400}
  alt="Hero"
  priority // Above the fold
  placeholder="blur"
/>
```

### Memoization
```tsx
// Expensive calculations
const filtered = useMemo(() =>
  items.filter(complexFilter),
  [items, filter]
);

// Stable callbacks
const handleClick = useCallback(() => {
  doSomething(id);
}, [id]);

// Component memoization
const MemoList = React.memo(List);
```

## Error Handling

### Error Boundaries
```tsx
// app/error.tsx
'use client';

export default function Error({
  error,
  reset,
}: {
  error: Error;
  reset: () => void;
}) {
  return (
    <div>
      <h2>Something went wrong!</h2>
      <button onClick={reset}>Try again</button>
    </div>
  );
}
```

### API Error Handling
```tsx
try {
  const data = await api.get('/endpoint');
} catch (error) {
  if (error instanceof ApiError) {
    if (error.status === 401) redirect('/login');
    if (error.status === 404) notFound();
  }
  throw error; // Let error boundary handle
}
```

## Testing Strategy

| Type | Tools | What to Test |
|------|-------|--------------|
| Unit | Jest, Vitest | Utils, hooks, pure functions |
| Component | RTL | Rendering, interactions |
| Integration | RTL + MSW | API flows, user journeys |
| E2E | Playwright | Critical paths, cross-browser |
| Visual | Chromatic | UI regressions |

## Security Checklist

- [ ] Validate all user inputs (Zod)
- [ ] Escape dynamic content (React does this)
- [ ] Use Content Security Policy
- [ ] Store tokens in httpOnly cookies
- [ ] Implement CSRF protection
- [ ] Sanitize URLs before navigation
- [ ] Avoid dangerouslySetInnerHTML

## Accessibility Checklist

- [ ] Semantic HTML elements
- [ ] ARIA labels where needed
- [ ] Keyboard navigation works
- [ ] Focus management
- [ ] Color contrast (4.5:1)
- [ ] Alt text for images
- [ ] Form labels linked
- [ ] Skip links present
