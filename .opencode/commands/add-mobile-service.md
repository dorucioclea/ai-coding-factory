# /add-mobile-service - Add API Service

Create an API service with TanStack Query hooks for React Native.

## Usage
```
/add-mobile-service <EntityName> [base-path] [options]
```

Examples:
- `/add-mobile-service Product /products`
- `/add-mobile-service Order /orders --crud`
- `/add-mobile-service User /users --auth-required`
- `/add-mobile-service Cart /cart --optimistic`

Options:
- `--crud` - Generate full CRUD operations
- `--auth-required` - Include auth token in requests
- `--optimistic` - Add optimistic update patterns
- `--paginated` - Add pagination support
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Parse Service Information

Extract:
- Entity name (singular, PascalCase)
- Base API path
- Operations needed
- Special patterns (optimistic, pagination)

### 2. Generate Type Definitions

```typescript
// types/<entity>.types.ts
export interface <Entity> {
  id: string;
  // Add entity properties
  createdAt: string;
  updatedAt: string;
}

export interface Create<Entity>Dto {
  // Create payload
}

export interface Update<Entity>Dto {
  // Update payload (Partial<Create>)
}

export interface <Entity>ListParams {
  page?: number;
  limit?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface <Entity>ListResponse {
  data: <Entity>[];
  meta: {
    total: number;
    page: number;
    limit: number;
    totalPages: number;
  };
}
```

### 3. Generate Service Functions

```typescript
// services/api/<entity>Service.ts
import { apiClient } from './apiClient';
import type {
  <Entity>,
  Create<Entity>Dto,
  Update<Entity>Dto,
  <Entity>ListParams,
  <Entity>ListResponse,
} from '@/types/<entity>.types';

export const <entity>Service = {
  // List with pagination
  async getAll(params?: <Entity>ListParams): Promise<<Entity>ListResponse> {
    const { data } = await apiClient.get('/<entities>', { params });
    return data;
  },

  // Get by ID
  async getById(id: string): Promise<<Entity>> {
    const { data } = await apiClient.get(`/<entities>/${id}`);
    return data;
  },

  // Create
  async create(dto: Create<Entity>Dto): Promise<<Entity>> {
    const { data } = await apiClient.post('/<entities>', dto);
    return data;
  },

  // Update
  async update(id: string, dto: Update<Entity>Dto): Promise<<Entity>> {
    const { data } = await apiClient.patch(`/<entities>/${id}`, dto);
    return data;
  },

  // Delete
  async delete(id: string): Promise<void> {
    await apiClient.delete(`/<entities>/${id}`);
  },
};
```

### 4. Generate TanStack Query Hooks

```typescript
// hooks/api/use<Entity>.ts
import {
  useQuery,
  useMutation,
  useQueryClient,
  useInfiniteQuery,
} from '@tanstack/react-query';
import { <entity>Service } from '@/services/api/<entity>Service';
import type {
  <Entity>,
  Create<Entity>Dto,
  Update<Entity>Dto,
  <Entity>ListParams,
} from '@/types/<entity>.types';

// Query Keys
export const <entity>Keys = {
  all: ['<entities>'] as const,
  lists: () => [...<entity>Keys.all, 'list'] as const,
  list: (params?: <Entity>ListParams) => [...<entity>Keys.lists(), params] as const,
  details: () => [...<entity>Keys.all, 'detail'] as const,
  detail: (id: string) => [...<entity>Keys.details(), id] as const,
};

// List Hook
export function use<Entity>List(params?: <Entity>ListParams) {
  return useQuery({
    queryKey: <entity>Keys.list(params),
    queryFn: () => <entity>Service.getAll(params),
  });
}

// Infinite List Hook (for pagination)
export function use<Entity>InfiniteList(params?: Omit<<Entity>ListParams, 'page'>) {
  return useInfiniteQuery({
    queryKey: <entity>Keys.list(params),
    queryFn: ({ pageParam = 1 }) =>
      <entity>Service.getAll({ ...params, page: pageParam }),
    getNextPageParam: (lastPage) =>
      lastPage.meta.page < lastPage.meta.totalPages
        ? lastPage.meta.page + 1
        : undefined,
    initialPageParam: 1,
  });
}

// Detail Hook
export function use<Entity>(id: string) {
  return useQuery({
    queryKey: <entity>Keys.detail(id),
    queryFn: () => <entity>Service.getById(id),
    enabled: !!id,
  });
}

// Create Mutation
export function useCreate<Entity>() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dto: Create<Entity>Dto) => <entity>Service.create(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: <entity>Keys.lists() });
    },
  });
}

// Update Mutation with Optimistic Updates
export function useUpdate<Entity>() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: Update<Entity>Dto }) =>
      <entity>Service.update(id, dto),
    onMutate: async ({ id, dto }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: <entity>Keys.detail(id) });

      // Snapshot current value
      const previous = queryClient.getQueryData<<Entity>>(<entity>Keys.detail(id));

      // Optimistically update
      queryClient.setQueryData<<Entity>>(<entity>Keys.detail(id), (old) =>
        old ? { ...old, ...dto } : old
      );

      return { previous };
    },
    onError: (err, { id }, context) => {
      // Rollback on error
      if (context?.previous) {
        queryClient.setQueryData(<entity>Keys.detail(id), context.previous);
      }
    },
    onSettled: (_, __, { id }) => {
      // Refetch to ensure consistency
      queryClient.invalidateQueries({ queryKey: <entity>Keys.detail(id) });
      queryClient.invalidateQueries({ queryKey: <entity>Keys.lists() });
    },
  });
}

// Delete Mutation
export function useDelete<Entity>() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => <entity>Service.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: <entity>Keys.lists() });
    },
  });
}
```

### 5. Generate Tests

```typescript
// hooks/api/__tests__/use<Entity>.test.ts
import { renderHook, waitFor } from '@testing-library/react-native';
import { createWrapper } from '@/test/utils';
import { use<Entity>List, use<Entity>, useCreate<Entity> } from '../use<Entity>';
import { server } from '@/mocks/server';
import { rest } from 'msw';

describe('use<Entity> hooks', () => {
  describe('use<Entity>List', () => {
    it('fetches <entities> successfully', async () => {
      const { result } = renderHook(() => use<Entity>List(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data?.data).toHaveLength(3);
    });
  });

  describe('use<Entity>', () => {
    it('fetches single <entity>', async () => {
      const { result } = renderHook(() => use<Entity>('1'), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data?.id).toBe('1');
    });
  });

  describe('useCreate<Entity>', () => {
    it('creates <entity> and invalidates list', async () => {
      const { result } = renderHook(() => useCreate<Entity>(), {
        wrapper: createWrapper(),
      });

      await result.current.mutateAsync({ /* dto */ });

      expect(result.current.isSuccess).toBe(true);
    });
  });
});
```

## Output

```markdown
## Service Created: <Entity>Service

### Files Created
- `types/<entity>.types.ts` - TypeScript interfaces
- `services/api/<entity>Service.ts` - API functions
- `hooks/api/use<Entity>.ts` - TanStack Query hooks
- `hooks/api/__tests__/use<Entity>.test.ts` - Tests
- `mocks/handlers/<entity>.ts` - MSW handlers (if testing)

### Available Hooks
- `use<Entity>List(params?)` - Fetch paginated list
- `use<Entity>(id)` - Fetch single entity
- `useCreate<Entity>()` - Create mutation
- `useUpdate<Entity>()` - Update mutation (optimistic)
- `useDelete<Entity>()` - Delete mutation

### Query Keys
- `<entity>Keys.all` - All <entity> queries
- `<entity>Keys.list(params)` - List queries
- `<entity>Keys.detail(id)` - Detail queries

### Usage Example
```typescript
import { use<Entity>List, useCreate<Entity> } from '@/hooks/api/use<Entity>';

function <Entity>ListScreen() {
  const { data, isLoading } = use<Entity>List();
  const createMutation = useCreate<Entity>();

  const handleCreate = async () => {
    await createMutation.mutateAsync({ /* dto */ });
  };
}
```
```

## Example

```
User: /add-mobile-service Product /products --crud --optimistic

Claude: Creating Product service with CRUD operations...

Files created:
- types/product.types.ts
- services/api/productService.ts
- hooks/api/useProduct.ts
- hooks/api/__tests__/useProduct.test.ts

Available hooks:
- useProductList() - with pagination
- useProduct(id) - single product
- useCreateProduct() - create mutation
- useUpdateProduct() - with optimistic updates
- useDeleteProduct() - delete mutation
```
