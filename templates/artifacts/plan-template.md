# Implementation Plan: {FEATURE_NAME}

**Branch**: `{STORY_ID}-{feature-slug}`
**Created**: {DATE}
**Spec**: [link to spec.md]
**Status**: Draft | Approved | In Progress | Complete

---

## Summary

{2-3 sentences: What we're building + high-level technical approach}

---

## Technical Context

| Aspect | Decision | Notes |
|--------|----------|-------|
| **Language/Version** | .NET 8 / TypeScript 5.x | From templates |
| **Backend Framework** | ASP.NET Core + MediatR | Clean Architecture |
| **Frontend Framework** | Next.js 14 (App Router) | React 18 |
| **Database** | PostgreSQL 16 | EF Core 8 |
| **Cache** | Redis 7 | Optional |
| **Authentication** | JWT + Refresh Tokens | Cookie-based storage |
| **Testing** | xUnit + Vitest + Playwright | TDD approach |

---

## Constitution Check

*Verify against project constitution before proceeding*

| Principle | Status | Notes |
|-----------|--------|-------|
| Clean Architecture | ✅ Pass | Layers properly separated |
| Test-First | ✅ Pass | Test plan included |
| {Principle 3} | ✅ Pass | {verification} |
| {Principle 4} | ⚠️ Deviation | {justification required} |

**If any deviations, document in Complexity Tracking section below.**

---

## Project Structure

### Documentation (this feature)

```
specs/{STORY_ID}-{feature}/
├── spec.md              # Feature specification
├── plan.md              # This file
├── tasks.md             # Task breakdown (from /speckit.tasks)
├── research.md          # Technical research (if needed)
└── checklists/
    └── requirements.md  # Quality checklist
```

### Source Code Changes

#### Backend (`{ProjectName}-api/`)

```
src/{ProjectName}.Domain/
├── Entities/
│   └── {Entity}.cs              # New entity
└── Repositories/
    └── I{Entity}Repository.cs   # Repository interface

src/{ProjectName}.Application/
├── Commands/{Feature}/
│   ├── Create{Entity}Command.cs
│   ├── Create{Entity}Handler.cs
│   └── Create{Entity}Validator.cs
├── Queries/{Feature}/
│   ├── Get{Entity}Query.cs
│   └── Get{Entity}Handler.cs
└── DTOs/
    └── {Entity}Dto.cs

src/{ProjectName}.Infrastructure/
├── Repositories/
│   └── {Entity}Repository.cs    # Repository implementation
└── Persistence/Configurations/
    └── {Entity}Configuration.cs # EF Core config

src/{ProjectName}.Api/
└── Controllers/
    └── {Entity}Controller.cs    # API endpoints
```

#### Frontend (`{ProjectName}-frontend/`)

```
src/
├── types/
│   └── {entity}.ts              # TypeScript types
├── hooks/
│   └── use-{entity}.ts          # TanStack Query hooks
├── components/features/{entity}/
│   ├── {Entity}List.tsx
│   ├── {Entity}Card.tsx
│   └── {Entity}Form.tsx
└── app/dashboard/{entity}/
    └── page.tsx                 # Route page
```

---

## Data Model

### New Entities

```
┌─────────────────────────────────────┐
│            {Entity}                  │
├─────────────────────────────────────┤
│ Id: Guid (PK)                       │
│ {Field1}: {Type}                    │
│ {Field2}: {Type}                    │
│ CreatedAt: DateTime                 │
│ UpdatedAt: DateTime?                │
│ CreatedBy: Guid (FK → User)         │
└─────────────────────────────────────┘
          │
          │ 1:N
          ▼
┌─────────────────────────────────────┐
│         {RelatedEntity}              │
├─────────────────────────────────────┤
│ Id: Guid (PK)                       │
│ {Entity}Id: Guid (FK)               │
│ {Field}: {Type}                     │
└─────────────────────────────────────┘
```

### Database Migrations

- [ ] `{timestamp}_Add{Entity}Table.cs`
- [ ] `{timestamp}_Add{RelatedEntity}Table.cs`

---

## API Design

### Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/{entities}` | List all (paginated) | Optional |
| GET | `/api/{entities}/{id}` | Get by ID | Optional |
| POST | `/api/{entities}` | Create new | Required |
| PUT | `/api/{entities}/{id}` | Update | Required (owner) |
| DELETE | `/api/{entities}/{id}` | Delete | Required (owner) |

### Request/Response Examples

**POST /api/{entities}**
```json
// Request
{
  "field1": "value",
  "field2": 123
}

// Response (201 Created)
{
  "id": "guid",
  "field1": "value",
  "field2": 123,
  "createdAt": "2024-01-23T12:00:00Z"
}
```

---

## Implementation Phases

### Phase 1: Foundation (Backend Core)

**Goal**: Basic entity and CRUD operations working

- Domain entity with validation
- Repository interface and implementation
- EF Core configuration and migration
- Basic API endpoints

**Deliverable**: API endpoints functional via Swagger

### Phase 2: Business Logic

**Goal**: Full feature logic implemented

- Command handlers with validation
- Query handlers with pagination
- Authorization rules
- Error handling

**Deliverable**: All acceptance criteria testable via API

### Phase 3: Frontend Integration

**Goal**: UI connected to backend

- TypeScript types matching API
- TanStack Query hooks
- Form with validation
- List/detail views

**Deliverable**: Feature usable through UI

### Phase 4: Polish

**Goal**: Production-ready

- Loading states and error handling
- Edge cases covered
- E2E tests passing
- Documentation updated

**Deliverable**: Feature ready for release

---

## Testing Strategy

### Backend Tests

| Type | Location | Coverage Target |
|------|----------|-----------------|
| Unit | `tests/{Project}.UnitTests/` | Domain: 90%, Application: 80% |
| Integration | `tests/{Project}.IntegrationTests/` | All endpoints |
| Architecture | `tests/{Project}.ArchitectureTests/` | Layer rules |

### Frontend Tests

| Type | Location | Coverage Target |
|------|----------|-----------------|
| Unit | `src/__tests__/` | Hooks, utils: 80% |
| Component | `src/__tests__/components/` | Key components |
| E2E | `e2e/` | Critical user flows |

---

## Complexity Tracking

> **Only fill if Constitution Check has deviations**

| Deviation | Why Needed | Simpler Alternative Rejected |
|-----------|------------|------------------------------|
| {violation} | {current need} | {why alternative insufficient} |

---

## Dependencies

### External Dependencies

- [ ] {External API/service if any}
- [ ] {Third-party library if any}

### Internal Dependencies

- [ ] User/Auth system (existing)
- [ ] {Other feature if any}

---

## Risks & Mitigations

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| {Risk 1} | High/Med/Low | High/Med/Low | {How to address} |

---

## Open Questions

- [ ] {Technical question needing research}
- [ ] {Design decision needing input}

---

## Approval

- [ ] Technical lead review
- [ ] Architecture compliance verified
- [ ] Ready for task breakdown

**Approved by**: _______________
**Date**: _______________
