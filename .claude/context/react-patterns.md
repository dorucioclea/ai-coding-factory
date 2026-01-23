# React Patterns Quick Reference

## Component Patterns

### Functional Components (Preferred)
```tsx
import { FC } from 'react';

interface Props {
  title: string;
  children?: React.ReactNode;
}

export const MyComponent: FC<Props> = ({ title, children }) => {
  return (
    <div>
      <h1>{title}</h1>
      {children}
    </div>
  );
};
```

### Compound Components
```tsx
const Card = ({ children }) => <div className="card">{children}</div>;
Card.Header = ({ children }) => <header>{children}</header>;
Card.Body = ({ children }) => <main>{children}</main>;

// Usage
<Card>
  <Card.Header>Title</Card.Header>
  <Card.Body>Content</Card.Body>
</Card>
```

### Render Props
```tsx
interface RenderProps<T> {
  data: T;
  render: (data: T) => React.ReactNode;
}

const DataRenderer = <T,>({ data, render }: RenderProps<T>) => {
  return <>{render(data)}</>;
};
```

## Hook Patterns

### Custom Hook Structure
```tsx
function useCustomHook(initialValue: T) {
  const [value, setValue] = useState(initialValue);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const action = useCallback(async () => {
    setIsLoading(true);
    try {
      // Do something
    } catch (e) {
      setError(e);
    } finally {
      setIsLoading(false);
    }
  }, []);

  return { value, error, isLoading, action };
}
```

### Rules of Hooks
- ✅ Call at top level only
- ✅ Call from React functions only
- ❌ Never in loops/conditions/nested functions

## State Management

### useState
```tsx
const [state, setState] = useState(initial);
setState(newValue);              // Direct
setState(prev => prev + 1);      // Functional (safer)
```

### useReducer (Complex State)
```tsx
type Action = { type: 'increment' } | { type: 'decrement' };
const reducer = (state: number, action: Action) => {
  switch (action.type) {
    case 'increment': return state + 1;
    case 'decrement': return state - 1;
  }
};
const [count, dispatch] = useReducer(reducer, 0);
```

### Context (Global State)
```tsx
const ThemeContext = createContext<Theme>('light');

// Provider
<ThemeContext.Provider value={theme}>
  {children}
</ThemeContext.Provider>

// Consumer
const theme = useContext(ThemeContext);
```

## Effect Patterns

### useEffect Dependencies
```tsx
useEffect(() => {
  // Runs after every render
});

useEffect(() => {
  // Runs once on mount
}, []);

useEffect(() => {
  // Runs when dep changes
}, [dep]);

useEffect(() => {
  // Setup
  return () => {
    // Cleanup
  };
}, []);
```

### Common Mistakes
```tsx
// ❌ Missing dependency
useEffect(() => {
  fetch(`/api/${id}`);
}, []); // id is missing

// ✅ Include all dependencies
useEffect(() => {
  fetch(`/api/${id}`);
}, [id]);
```

## Performance Patterns

### useMemo (Expensive Calculations)
```tsx
const expensive = useMemo(() => {
  return computeExpensiveValue(a, b);
}, [a, b]);
```

### useCallback (Stable References)
```tsx
const handleClick = useCallback(() => {
  doSomething(id);
}, [id]);
```

### React.memo (Prevent Re-renders)
```tsx
const MemoizedComponent = React.memo(({ prop }) => {
  return <div>{prop}</div>;
});
```

## Form Patterns

### Controlled Components
```tsx
const [value, setValue] = useState('');
<input
  value={value}
  onChange={(e) => setValue(e.target.value)}
/>
```

### React Hook Form + Zod
```tsx
const schema = z.object({
  email: z.string().email(),
});

const { register, handleSubmit } = useForm({
  resolver: zodResolver(schema),
});
```

## Error Handling

### Error Boundaries
```tsx
class ErrorBoundary extends React.Component {
  state = { hasError: false };

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  render() {
    if (this.state.hasError) {
      return <ErrorFallback />;
    }
    return this.props.children;
  }
}
```

### Suspense
```tsx
<Suspense fallback={<Loading />}>
  <LazyComponent />
</Suspense>
```

## Testing Patterns

### React Testing Library
```tsx
import { render, screen, fireEvent } from '@testing-library/react';

test('renders correctly', () => {
  render(<Component />);
  expect(screen.getByText('Hello')).toBeInTheDocument();
});

test('handles interaction', async () => {
  const user = userEvent.setup();
  render(<Button onClick={handleClick} />);
  await user.click(screen.getByRole('button'));
  expect(handleClick).toHaveBeenCalled();
});
```

## Anti-Patterns to Avoid

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| Prop drilling | Hard to maintain | Use Context |
| Inline objects/functions | Causes re-renders | useMemo/useCallback |
| Index as key | Breaks updates | Use stable IDs |
| Direct DOM manipulation | Bypasses React | Use refs properly |
| Mutating state directly | No re-render | Use setState |
