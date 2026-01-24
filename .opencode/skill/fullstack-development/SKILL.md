---
name: fullstack-development
description: Use when building features that span both frontend and backend, or when setting up a new fullstack project. Orchestrates the clean-architecture-solution backend with react-frontend-template.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  stack: dotnet-react
---

# Fullstack Development

## Overview

Guide for developing fullstack applications using the .NET backend (clean-architecture-solution) and React frontend (react-frontend-template) together. Ensures consistent patterns across the stack.

## When to Use

- Setting up a new fullstack project
- Adding a feature that needs both API endpoint and UI
- Implementing authentication flows
- Connecting frontend forms to backend APIs
- Debugging cross-stack issues

## Project Structure

A typical fullstack project has this structure:

```
projects/
+-- {ProjectName}-api/           # .NET backend (from clean-architecture-solution)
|   +-- src/
|   |   +-- {ProjectName}.Domain/
|   |   +-- {ProjectName}.Application/
|   |   +-- {ProjectName}.Infrastructure/
|   |   +-- {ProjectName}.Api/
|   +-- tests/
+-- {ProjectName}-frontend/      # React frontend (from react-frontend-template)
|   +-- src/
|   |   +-- app/
|   |   +-- components/
|   |   +-- lib/
|   |   +-- hooks/
|   +-- e2e/
+-- infrastructure/              # Shared Docker setup
    +-- docker-compose.yml
    +-- .env
```

## Setting Up New Fullstack Project

### Step 1: Scaffold Both Templates

```bash
# Backend
/scaffold {ProjectName} clean-architecture

# Frontend
/scaffold {ProjectName} react-frontend
```

### Step 2: Copy Infrastructure

```bash
cp -r templates/infrastructure projects/{ProjectName}/infrastructure
cd projects/{ProjectName}/infrastructure
cp .env.example .env
```

### Step 3: Configure Environment

Edit `projects/{ProjectName}/infrastructure/.env`:

```bash
PROJECT_NAME={projectname}
JWT_SECRET=generate-a-secure-32-char-secret
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

### Step 4: Start Development

```bash
# Terminal 1: Infrastructure
cd projects/{ProjectName}/infrastructure
docker compose up -d

# Terminal 2: Backend
cd projects/{ProjectName}-api
dotnet run --project src/{ProjectName}.Api

# Terminal 3: Frontend
cd projects/{ProjectName}-frontend
npm install && npm run dev
```

## Adding a New Feature (Vertical Slice)

When adding a feature like "Fishing Spots", implement across the stack:

### 1. Backend: Domain Entity

```csharp
// Domain/Entities/FishingSpot.cs
public class FishingSpot : Entity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    public static FishingSpot Create(string name, decimal lat, decimal lng)
    {
        return new FishingSpot { Name = name, Latitude = lat, Longitude = lng };
    }
}
```

### 2. Backend: CQRS Command

```csharp
// Application/Commands/FishingSpots/CreateFishingSpotCommand.cs
public record CreateFishingSpotCommand(
    string Name,
    decimal Latitude,
    decimal Longitude
) : IRequest<Result<Guid>>;
```

### 3. Backend: API Endpoint

```csharp
// Api/Controllers/FishingSpotsController.cs
[HttpPost]
public async Task<IActionResult> Create(CreateFishingSpotCommand command, CancellationToken ct)
{
    var result = await _mediator.Send(command, ct);
    return result.IsSuccess
        ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
        : BadRequest(result.Error);
}
```

### 4. Frontend: Type Definitions

```typescript
// types/fishing-spot.ts
export interface FishingSpot {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  createdAt: string;
}

export interface CreateFishingSpotDto {
  name: string;
  latitude: number;
  longitude: number;
}
```

### 5. Frontend: API Hook

```typescript
// hooks/use-fishing-spots.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type { FishingSpot, CreateFishingSpotDto } from '@/types/fishing-spot';

export function useFishingSpots() {
  return useQuery({
    queryKey: queryKeys.entity('fishingSpots').all,
    queryFn: () => apiClient.get<FishingSpot[]>('/fishing-spots'),
  });
}

export function useCreateFishingSpot() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateFishingSpotDto) =>
      apiClient.post<{ id: string }>('/fishing-spots', data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.entity('fishingSpots').all,
      });
    },
  });
}
```

### 6. Frontend: Form Component

```tsx
// components/features/fishing-spots/SpotForm.tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useCreateFishingSpot } from '@/hooks/use-fishing-spots';
import { FormField } from '@/components/forms';
import { Button } from '@/components/ui';

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  latitude: z.number().min(-90).max(90),
  longitude: z.number().min(-180).max(180),
});

export function SpotForm({ onSuccess }: { onSuccess?: () => void }) {
  const { mutate, isPending } = useCreateFishingSpot();
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: z.infer<typeof schema>) => {
    mutate(data, { onSuccess });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <FormField control={control} name="name" label="Spot Name" />
      <FormField control={control} name="latitude" label="Latitude" type="number" />
      <FormField control={control} name="longitude" label="Longitude" type="number" />
      <Button type="submit" disabled={isPending}>
        {isPending ? 'Creating...' : 'Create Spot'}
      </Button>
    </form>
  );
}
```

## API Contract Pattern

### Backend Response Format

The backend should return consistent responses:

```csharp
// Success
{ "success": true, "data": { ... } }

// Error
{ "success": false, "error": "Error message", "details": { ... } }

// Paginated
{ "success": true, "data": [...], "meta": { "total": 100, "page": 1, "limit": 10 } }
```

### Frontend Types Match

```typescript
// types/api.ts
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  meta?: {
    total: number;
    page: number;
    limit: number;
  };
}
```

## Testing Strategy

### Backend: Unit + Integration

```csharp
// Unit test command handler
[Fact]
public async Task CreateFishingSpot_ValidData_ReturnsId()
{
    var handler = new CreateFishingSpotHandler(_repo.Object, _uow.Object);
    var result = await handler.Handle(command, CancellationToken.None);
    result.IsSuccess.Should().BeTrue();
}

// Integration test API
[Fact]
public async Task POST_FishingSpots_Returns201()
{
    var response = await _client.PostAsJsonAsync("/api/fishing-spots", dto);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Frontend: Component + E2E

```typescript
// Component test
describe('SpotForm', () => {
  it('submits valid data', async () => {
    render(<SpotForm />);
    await userEvent.type(screen.getByLabelText(/name/i), 'Lake Spot');
    await userEvent.click(screen.getByRole('button', { name: /create/i }));
    expect(mockMutate).toHaveBeenCalled();
  });
});

// E2E test
test('can create fishing spot', async ({ page }) => {
  await page.goto('/dashboard/spots');
  await page.fill('[name="name"]', 'Lake Spot');
  await page.click('button[type="submit"]');
  await expect(page.getByText('Lake Spot')).toBeVisible();
});
```

## Quick Reference

### Start Everything

```bash
cd projects/{ProjectName}/infrastructure
docker compose up -d           # DB + Redis
cd ../api && dotnet run        # Backend
cd ../frontend && npm run dev  # Frontend
```

### Common URLs

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| pgAdmin | http://localhost:5050 |

### Related Skills

- `dotnet-clean-architecture` - Backend patterns
- `react-scaffold` - Frontend patterns
- `docker-infrastructure` - Docker management
