# /implement - Implement a User Story

Full implementation workflow for a user story including code, tests, and documentation.

## Usage
```
/implement <story-id>
```

Example: `/implement ACF-042`

## Instructions

When invoked, follow this structured implementation workflow:

### Phase 1: Story Analysis

1. **Read the story file**:
   ```bash
   cat artifacts/stories/<story-id>.md
   ```

2. **Verify Definition of Ready**:
   - Story follows INVEST criteria
   - Acceptance criteria are testable
   - Dependencies identified
   - No blocking questions

3. **If not ready**: Stop and report what's missing

### Phase 2: Technical Design

1. **Identify affected layers**:
   - Domain: New entities, value objects, domain events?
   - Application: Commands, queries, handlers, validators?
   - Infrastructure: Repositories, external services?
   - API: Endpoints, DTOs, middleware?

2. **Create/update ADR if needed**:
   - New dependencies → ADR required
   - Architecture changes → ADR required
   - Breaking changes → ADR required

3. **List files to create/modify**:
   - Provide a clear list before writing code

### Phase 3: Implementation

1. **Domain Layer** (if applicable):
   ```csharp
   // Entities
   src/<Project>.Domain/Entities/<Entity>.cs

   // Value Objects
   src/<Project>.Domain/ValueObjects/<ValueObject>.cs

   // Domain Events
   src/<Project>.Domain/Events/<Event>.cs

   // Repository Interfaces
   src/<Project>.Domain/Repositories/I<Entity>Repository.cs
   ```

2. **Application Layer**:
   ```csharp
   // Commands
   src/<Project>.Application/Commands/<Feature>/
   ├── <Command>Command.cs
   ├── <Command>Handler.cs
   └── <Command>Validator.cs

   // Queries
   src/<Project>.Application/Queries/<Feature>/
   ├── <Query>Query.cs
   └── <Query>Handler.cs

   // DTOs
   src/<Project>.Application/DTOs/<Entity>Dto.cs
   ```

3. **Infrastructure Layer**:
   ```csharp
   // Repository Implementation
   src/<Project>.Infrastructure/Repositories/<Entity>Repository.cs

   // EF Configuration
   src/<Project>.Infrastructure/Persistence/Configurations/<Entity>Configuration.cs
   ```

4. **API Layer**:
   ```csharp
   // Controller
   src/<Project>.Api/Controllers/<Entity>Controller.cs
   ```

### Phase 4: Testing

1. **Unit Tests** (for Domain/Application):
   ```csharp
   [Fact]
   [Trait("Story", "<story-id>")]
   public async Task <MethodName>_<Scenario>_<ExpectedResult>()
   {
       // Arrange
       // Act
       // Assert
   }
   ```

2. **Integration Tests** (for API endpoints):
   ```csharp
   [Fact]
   [Trait("Story", "<story-id>")]
   public async Task <Endpoint>_<Scenario>_Returns<Status>()
   ```

3. **Run tests and check coverage**:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   python3 scripts/coverage/check-coverage.py coverage.xml
   ```

### Phase 5: Documentation

1. **Update API documentation**:
   - OpenAPI comments on endpoints
   - Example requests/responses

2. **Update module documentation** if structure changed

3. **Update architecture documentation** if significant changes

### Phase 6: Validation

1. **Run all validations**:
   ```bash
   ./scripts/validate-project.sh
   ./scripts/validate-documentation.sh
   python3 scripts/traceability/traceability.py validate
   ```

2. **Verify Definition of Done**:
   - [ ] All acceptance criteria met
   - [ ] Unit tests with >80% coverage
   - [ ] Integration tests for endpoints
   - [ ] Code reviewed
   - [ ] Documentation updated
   - [ ] Security review (if applicable)
   - [ ] Traceability verified

### Phase 7: Commit

```bash
git add .
git commit -m "<story-id> Implement <brief description>

- Added <feature 1>
- Updated <component>
- Tests: X unit, Y integration

Co-Authored-By: Claude <noreply@anthropic.com>"
```

## Output Format

```markdown
## Implementation Summary: <story-id>

### Story: <title>

### Changes Made

| Layer | Files | Description |
|-------|-------|-------------|
| Domain | 2 | Added Order entity, OrderStatus value object |
| Application | 4 | CreateOrderCommand, handler, validator, DTO |
| Infrastructure | 2 | OrderRepository, EF configuration |
| API | 1 | OrdersController with POST /orders |
| Tests | 6 | 5 unit tests, 1 integration test |

### Test Results
- Unit tests: 12 passed
- Integration tests: 3 passed
- Coverage: 87%

### Traceability
- Story: <story-id>
- Tests: 6 linked with [Trait("Story", "<story-id>")]
- Commits: 1 with story ID

### Definition of Done
- [x] Acceptance criteria met
- [x] Tests with 87% coverage
- [x] Documentation updated
- [x] Traceability verified

### Ready for Review: Yes
```

## Example

```
User: /implement ACF-042

Claude: Implementing story ACF-042...

## Story Analysis
Reading artifacts/stories/ACF-042.md...

**ACF-042: User Login**
As a registered user, I want to log in with email and password...

### Definition of Ready: PASSED
- [x] INVEST criteria met
- [x] Acceptance criteria testable
- [x] Dependencies: AuthService exists
- [x] No blocking questions

## Technical Design

**Affected Layers:**
- Application: LoginCommand, LoginHandler, LoginValidator
- Infrastructure: Update UserRepository with login method
- API: Add POST /auth/login endpoint

**Files to create/modify:**
1. src/Application/Commands/Auth/LoginCommand.cs (new)
2. src/Application/Commands/Auth/LoginHandler.cs (new)
3. src/Application/Commands/Auth/LoginValidator.cs (new)
4. src/Api/Controllers/AuthController.cs (modify)
5. tests/UnitTests/Auth/LoginHandlerTests.cs (new)
6. tests/IntegrationTests/Auth/LoginEndpointTests.cs (new)

Proceeding with implementation...
```
