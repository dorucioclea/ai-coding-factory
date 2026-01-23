# TypeScript Standards Quick Reference

## Type Definitions

### Prefer `interface` for Objects
```typescript
// ✅ Preferred for object shapes
interface User {
  id: string;
  name: string;
  email: string;
}

// Use type for unions, primitives, utilities
type Status = 'pending' | 'active' | 'inactive';
type ID = string | number;
```

### Generic Types
```typescript
// Generic function
function identity<T>(arg: T): T {
  return arg;
}

// Generic interface
interface ApiResponse<T> {
  data: T;
  status: number;
  message: string;
}

// Generic with constraints
function getProperty<T, K extends keyof T>(obj: T, key: K): T[K] {
  return obj[key];
}
```

## Strict Mode Rules

### Enable in tsconfig.json
```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true,
    "noImplicitReturns": true
  }
}
```

### Handle Nullability
```typescript
// ❌ Avoid
function getName(user: User) {
  return user.name.toUpperCase(); // Could be null!
}

// ✅ Proper null handling
function getName(user: User | null): string {
  return user?.name?.toUpperCase() ?? 'Unknown';
}
```

## Type Guards

### typeof
```typescript
function process(value: string | number) {
  if (typeof value === 'string') {
    return value.toUpperCase();
  }
  return value.toFixed(2);
}
```

### instanceof
```typescript
function handleError(error: unknown) {
  if (error instanceof Error) {
    console.log(error.message);
  }
}
```

### Custom Type Guards
```typescript
interface Dog {
  bark(): void;
}

function isDog(animal: unknown): animal is Dog {
  return typeof (animal as Dog).bark === 'function';
}
```

## Utility Types

| Utility | Purpose | Example |
|---------|---------|---------|
| `Partial<T>` | All optional | `Partial<User>` |
| `Required<T>` | All required | `Required<User>` |
| `Readonly<T>` | All readonly | `Readonly<User>` |
| `Pick<T, K>` | Select keys | `Pick<User, 'id' \| 'name'>` |
| `Omit<T, K>` | Exclude keys | `Omit<User, 'password'>` |
| `Record<K, V>` | Key-value map | `Record<string, number>` |
| `NonNullable<T>` | Remove null | `NonNullable<string \| null>` |
| `ReturnType<T>` | Function return | `ReturnType<typeof fn>` |
| `Parameters<T>` | Function params | `Parameters<typeof fn>` |

## Discriminated Unions

```typescript
interface Loading {
  status: 'loading';
}

interface Success<T> {
  status: 'success';
  data: T;
}

interface Error {
  status: 'error';
  error: string;
}

type AsyncState<T> = Loading | Success<T> | Error;

function render(state: AsyncState<User>) {
  switch (state.status) {
    case 'loading':
      return <Spinner />;
    case 'success':
      return <UserCard user={state.data} />;
    case 'error':
      return <Error message={state.error} />;
  }
}
```

## Assertion Patterns

### Type Assertions (Use Sparingly)
```typescript
// When you know more than TypeScript
const input = document.getElementById('input') as HTMLInputElement;

// Prefer type guards when possible
const input = document.getElementById('input');
if (input instanceof HTMLInputElement) {
  input.value = 'hello';
}
```

### Non-null Assertion (Avoid)
```typescript
// ❌ Risky
const name = user!.name;

// ✅ Safer
const name = user?.name ?? 'default';
```

## Enums vs Union Types

```typescript
// ✅ Prefer union types (tree-shakable)
type Direction = 'north' | 'south' | 'east' | 'west';

// Use const enum only if needed
const enum Status {
  Pending = 'PENDING',
  Active = 'ACTIVE',
}

// Object as const (type-safe)
const COLORS = {
  red: '#ff0000',
  green: '#00ff00',
} as const;

type Color = keyof typeof COLORS;
```

## Module Patterns

### Named Exports (Preferred)
```typescript
// user.ts
export interface User { ... }
export function createUser() { ... }

// consumer.ts
import { User, createUser } from './user';
```

### Barrel Exports
```typescript
// index.ts
export * from './user';
export * from './product';
export { default as api } from './api';
```

## React-Specific TypeScript

### Props Types
```typescript
interface Props {
  // Required
  title: string;
  // Optional
  subtitle?: string;
  // Children
  children: React.ReactNode;
  // Event handlers
  onClick: (event: React.MouseEvent<HTMLButtonElement>) => void;
  // Style
  style?: React.CSSProperties;
  // Class
  className?: string;
}
```

### Generic Components
```typescript
interface ListProps<T> {
  items: T[];
  renderItem: (item: T) => React.ReactNode;
}

function List<T>({ items, renderItem }: ListProps<T>) {
  return <ul>{items.map(renderItem)}</ul>;
}
```

### Hook Types
```typescript
function useLocalStorage<T>(key: string, initial: T) {
  const [value, setValue] = useState<T>(initial);
  // ...
  return [value, setValue] as const;
}
```

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Using `any` | Use `unknown` and narrow |
| Object as `{}` | Use `Record<string, unknown>` |
| Array as `[]` | Use `unknown[]` or specific |
| Missing return types | Add explicit return types |
| Type assertions everywhere | Use type guards |
