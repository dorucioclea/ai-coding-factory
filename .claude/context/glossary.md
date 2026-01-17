# Domain Glossary

Standard terminology used in AI Coding Factory projects.

## Architecture Terms

| Term | Definition |
|------|------------|
| **Aggregate** | A cluster of domain objects that are treated as a single unit for data changes. Has a root entity. |
| **Aggregate Root** | The single entity in an aggregate through which external objects can reference the aggregate. |
| **Bounded Context** | A conceptual boundary within which a particular domain model is defined and applicable. |
| **Clean Architecture** | An architecture pattern with concentric layers where dependencies point inward. |
| **CQRS** | Command Query Responsibility Segregation - separating read and write operations. |
| **Domain Event** | An event that represents something that happened in the domain. |
| **DTO** | Data Transfer Object - an object that carries data between processes. |
| **Entity** | A domain object that has a distinct identity that persists over time. |
| **Repository** | An abstraction that provides collection-like access to domain objects. |
| **Unit of Work** | Maintains a list of objects affected by a business transaction. |
| **Value Object** | An immutable object that describes a characteristic without an identity. |

## Project Layers

| Layer | Purpose | Dependencies |
|-------|---------|--------------|
| **Domain** | Core business logic and entities | None (no external dependencies) |
| **Application** | Use cases, commands, queries, DTOs | Domain only |
| **Infrastructure** | Data access, external services | Domain, Application |
| **API** | HTTP interface, controllers, middleware | All layers (composition root) |

## Agile/Scrum Terms

| Term | Definition |
|------|------------|
| **ACF-###** | Story ID format used for traceability (AI Coding Factory). |
| **Acceptance Criteria** | Conditions that must be met for a story to be considered complete. |
| **Definition of Done (DoD)** | Checklist of criteria that all work must satisfy to be complete. |
| **Definition of Ready (DoR)** | Criteria a story must meet before it can be worked on. |
| **Epic** | A large body of work that can be broken down into stories. |
| **INVEST** | Criteria for good stories: Independent, Negotiable, Valuable, Estimable, Small, Testable. |
| **Sprint** | A time-boxed iteration (typically 2 weeks) for delivering increment. |
| **Story Points** | Relative measure of effort/complexity for a story. |
| **User Story** | A short description of a feature from the user's perspective. |
| **Velocity** | Measure of work a team can complete in a sprint (in story points). |

## Testing Terms

| Term | Definition |
|------|------------|
| **Arrange-Act-Assert** | Test structure pattern: set up, execute, verify. |
| **Code Coverage** | Percentage of code executed during tests (target: >=80%). |
| **Integration Test** | Test that verifies interactions between components. |
| **Mock** | A fake object that simulates the behavior of a real object. |
| **Test Pyramid** | Strategy: many unit tests, fewer integration tests, few E2E tests. |
| **Unit Test** | Test that verifies a single unit of code in isolation. |
| **xUnit** | Testing framework used for .NET projects. |

## .NET Terms

| Term | Definition |
|------|------------|
| **ASP.NET Core** | Framework for building web APIs and applications. |
| **DbContext** | Entity Framework Core class for database interactions. |
| **Dependency Injection (DI)** | Pattern where dependencies are provided to objects rather than created by them. |
| **Entity Framework Core** | ORM (Object-Relational Mapper) for .NET. |
| **FluentValidation** | Library for building validation rules. |
| **ILogger** | Interface for structured logging in .NET. |
| **MediatR** | Library implementing mediator pattern for CQRS. |
| **Middleware** | Components that process HTTP requests and responses. |

## DevOps Terms

| Term | Definition |
|------|------------|
| **ADR** | Architecture Decision Record - documents architectural decisions. |
| **CI/CD** | Continuous Integration / Continuous Deployment. |
| **Docker** | Container platform for packaging applications. |
| **Kubernetes (K8s)** | Container orchestration platform. |
| **Quality Gate** | Automated checkpoint that code must pass. |
| **Traceability** | Ability to trace work from requirement to deployment. |

## Security Terms

| Term | Definition |
|------|------------|
| **JWT** | JSON Web Token - compact token format for authentication. |
| **HTTPS** | HTTP over TLS/SSL encryption. |
| **OWASP** | Open Web Application Security Project - security standards. |
| **Parameterized Query** | Query that separates SQL from data to prevent injection. |
| **RBAC** | Role-Based Access Control. |

## Abbreviations

| Abbreviation | Full Form |
|--------------|-----------|
| ACF | AI Coding Factory |
| API | Application Programming Interface |
| CQRS | Command Query Responsibility Segregation |
| DDD | Domain-Driven Design |
| DoD | Definition of Done |
| DoR | Definition of Ready |
| DTO | Data Transfer Object |
| EF | Entity Framework |
| JWT | JSON Web Token |
| MCP | Model Context Protocol |
| PR | Pull Request |
| SUT | System Under Test |
| TDD | Test-Driven Development |
