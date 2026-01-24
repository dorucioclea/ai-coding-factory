# Tasks: {FEATURE_NAME}

**Feature**: `{STORY_ID}-{feature-slug}`
**Plan**: [link to plan.md]
**Spec**: [link to spec.md]
**Created**: {DATE}

---

## Task Format

```
[ID] [P?] [Story] Description
```

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1, US2, etc.) - enables independent delivery
- **Status**: ⬜ Todo | 🔄 In Progress | ✅ Done | ⏸️ Blocked

---

## Phase 1: Setup & Infrastructure

**Goal**: Project ready for feature development
**Checkpoint**: Can run backend + frontend locally

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T001 | | Setup infrastructure: `docker compose up -d` |
| ⬜ | T002 | [P] | Create feature branch: `{STORY_ID}-{feature-slug}` |
| ⬜ | T003 | [P] | Add any new NuGet/npm packages needed |

**⏸️ CHECKPOINT**: Infrastructure running, branch created

---

## Phase 2: Backend Foundation

**Goal**: Domain model and database ready
**Checkpoint**: Migration applied, entities in DB

| Status | ID | Par | Story | Task |
|--------|-----|-----|-------|------|
| ⬜ | T010 | | | Create `{Entity}.cs` in `Domain/Entities/` |
| ⬜ | T011 | [P] | | Create `{Entity}Id` value object (if using strong IDs) |
| ⬜ | T012 | | | Create `I{Entity}Repository.cs` in `Domain/Repositories/` |
| ⬜ | T013 | | | Create `{Entity}Configuration.cs` in `Infrastructure/Persistence/` |
| ⬜ | T014 | | | Create `{Entity}Repository.cs` in `Infrastructure/Repositories/` |
| ⬜ | T015 | | | Add DbSet to `ApplicationDbContext` |
| ⬜ | T016 | | | Create and apply EF migration |
| ⬜ | T017 | | | Register repository in DI container |

**⏸️ CHECKPOINT**: `dotnet ef database update` succeeds, entity visible in pgAdmin

---

## Phase 3: User Story 1 - {US1 Title} (P1 - MVP) 🎯

**Goal**: {What US1 delivers}
**Independent Test**: {How to verify US1 works alone}

### Tests First (TDD - Write these FIRST, they should FAIL)

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T020 | [P] | [US1] Unit test: `Create{Entity}Handler` in `UnitTests/` |
| ⬜ | T021 | [P] | [US1] Unit test: `{Entity}` domain validation |
| ⬜ | T022 | [P] | [US1] Integration test: `POST /api/{entities}` |
| ⬜ | T023 | [P] | [US1] Integration test: `GET /api/{entities}/{id}` |

### Implementation (Make tests PASS)

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T030 | | [US1] Create `Create{Entity}Command.cs` |
| ⬜ | T031 | | [US1] Create `Create{Entity}Validator.cs` |
| ⬜ | T032 | | [US1] Create `Create{Entity}Handler.cs` |
| ⬜ | T033 | [P] | [US1] Create `Get{Entity}Query.cs` |
| ⬜ | T034 | [P] | [US1] Create `Get{Entity}Handler.cs` |
| ⬜ | T035 | | [US1] Create `{Entity}Dto.cs` |
| ⬜ | T036 | | [US1] Create `{Entities}Controller.cs` with POST, GET endpoints |
| ⬜ | T037 | | [US1] Verify all tests pass |

**⏸️ CHECKPOINT**: US1 fully functional via Swagger, all tests green

---

## Phase 4: User Story 2 - {US2 Title} (P2)

**Goal**: {What US2 delivers}
**Independent Test**: {How to verify US2 works alone}

### Tests First

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T040 | [P] | [US2] Unit test: `Update{Entity}Handler` |
| ⬜ | T041 | [P] | [US2] Integration test: `PUT /api/{entities}/{id}` |

### Implementation

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T050 | | [US2] Create `Update{Entity}Command.cs` |
| ⬜ | T051 | | [US2] Create `Update{Entity}Handler.cs` |
| ⬜ | T052 | | [US2] Add PUT endpoint to controller |
| ⬜ | T053 | | [US2] Verify all tests pass |

**⏸️ CHECKPOINT**: US1 + US2 both functional

---

## Phase 5: User Story 3 - {US3 Title} (P3)

**Goal**: {What US3 delivers}

### Tests First

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T060 | [P] | [US3] Unit test: `Delete{Entity}Handler` |
| ⬜ | T061 | [P] | [US3] Integration test: `DELETE /api/{entities}/{id}` |

### Implementation

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T070 | | [US3] Create `Delete{Entity}Command.cs` |
| ⬜ | T071 | | [US3] Create `Delete{Entity}Handler.cs` |
| ⬜ | T072 | | [US3] Add DELETE endpoint to controller |
| ⬜ | T073 | | [US3] Verify all tests pass |

**⏸️ CHECKPOINT**: All backend stories complete

---

## Phase 6: Frontend Integration

**Goal**: UI connected to backend
**Checkpoint**: Feature usable through browser

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T080 | | Create `types/{entity}.ts` with TypeScript interfaces |
| ⬜ | T081 | | Create `hooks/use-{entity}.ts` with TanStack Query |
| ⬜ | T082 | [P] | Create `components/features/{entity}/{Entity}Card.tsx` |
| ⬜ | T083 | [P] | Create `components/features/{entity}/{Entity}Form.tsx` |
| ⬜ | T084 | | Create `components/features/{entity}/{Entity}List.tsx` |
| ⬜ | T085 | | Create `app/dashboard/{entity}/page.tsx` |
| ⬜ | T086 | | Add navigation link in sidebar |
| ⬜ | T087 | | Test full flow in browser |

**⏸️ CHECKPOINT**: Feature fully usable in UI

---

## Phase 7: E2E & Polish

**Goal**: Production-ready quality
**Checkpoint**: All tests pass, no console errors

| Status | ID | Par | Task |
|--------|-----|-----|------|
| ⬜ | T090 | | [E2E] Create `e2e/{entity}.spec.ts` |
| ⬜ | T091 | | [E2E] Test create flow |
| ⬜ | T092 | | [E2E] Test list/view flow |
| ⬜ | T093 | [P] | Add loading skeletons |
| ⬜ | T094 | [P] | Add error boundaries |
| ⬜ | T095 | [P] | Add toast notifications |
| ⬜ | T096 | | Final code review |
| ⬜ | T097 | | Update API documentation |

**⏸️ CHECKPOINT**: Feature complete, ready for PR

---

## Execution Order & Dependencies

### Dependency Graph

```
Phase 1 (Setup)
    │
    ▼
Phase 2 (Backend Foundation) ──── BLOCKS ALL ────┐
    │                                            │
    ▼                                            │
Phase 3 (US1) ◄──────────────────────────────────┤
    │                                            │
    ├── Phase 4 (US2) can start in parallel ◄────┤
    │                                            │
    └── Phase 5 (US3) can start in parallel ◄────┘
             │
             ▼
      Phase 6 (Frontend) - needs backend complete
             │
             ▼
      Phase 7 (E2E & Polish)
```

### Parallel Opportunities

**Within Phase 2** (Backend Foundation):
- T011 (Value object) can run parallel with T010 (Entity)
- After entity created, T012-T017 are sequential

**Within Phases 3-5** (User Stories):
- All test tasks marked [P] can run in parallel
- Different user stories can be worked on by different developers

**Within Phase 6** (Frontend):
- T082 and T083 (Card and Form components) can run in parallel

---

## Implementation Strategy Options

### Option A: MVP First (Recommended for solo dev)

1. Complete Phases 1-3 (US1 only)
2. **STOP** - Deploy/demo US1
3. Get feedback
4. Continue with US2, US3
5. Frontend
6. Polish

### Option B: Full Backend First

1. Complete Phases 1-5 (all backend)
2. Complete Phase 6 (all frontend)
3. Complete Phase 7 (polish)

### Option C: Parallel Team

- Dev A: Backend (Phases 2-5)
- Dev B: Frontend (Phase 6, after Phase 2 done)
- Both: Polish (Phase 7)

---

## Progress Tracking

| Phase | Tasks | Done | Progress |
|-------|-------|------|----------|
| 1. Setup | 3 | 0 | ⬜⬜⬜ |
| 2. Foundation | 8 | 0 | ⬜⬜⬜⬜⬜⬜⬜⬜ |
| 3. US1 (MVP) | 11 | 0 | ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ |
| 4. US2 | 6 | 0 | ⬜⬜⬜⬜⬜⬜ |
| 5. US3 | 6 | 0 | ⬜⬜⬜⬜⬜⬜ |
| 6. Frontend | 8 | 0 | ⬜⬜⬜⬜⬜⬜⬜⬜ |
| 7. Polish | 8 | 0 | ⬜⬜⬜⬜⬜⬜⬜⬜ |
| **Total** | **50** | **0** | **0%** |

---

## Notes

- Commit after each task or logical group
- Run tests before moving to next task
- Update this file as tasks complete
- Stop at any checkpoint to validate independently
