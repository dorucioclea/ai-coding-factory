# Skill Index

This is a reference of available skills. Each skill has its full content at the path shown.
**To use a skill:** Read the skill file at the path indicated, then follow its instructions.

---

## All Skills

| Skill | Description | File |
|-------|-------------|------|
| `add-serena` | This skill helps you add Serena MCP (Model Context Protocol) to any software project, providing IDE-like semantic code understanding and navigation capabilities with true multi-project support. | `skills/add-serena.md` |
| `akka-net-aspire-configuration` | Configure Akka.NET with .NET Aspire for local development and production deployments. Covers actor system setup, clustering, persistence, Akka.Management integration, and Aspire orchestration patterns. | `skills/akka-net-aspire-configuration.md` |
| `akka-net-testing-patterns` | Write unit and integration tests for Akka.NET actors using modern Akka.Hosting.TestKit patterns. Covers dependency injection, TestProbes, persistence testing, and actor interaction verification. Includes guidance on when to use traditional TestKit. | `skills/akka-net-testing-patterns.md` |
| `api-documentation` | Auto-generate comprehensive API documentation with examples, schemas, and interactive tools. | `skills/api-documentation.md` |
| `api-tester` | Quick API endpoint testing with comprehensive request/response validation. | `skills/api-tester.md` |
| `architecture-documenter` | Document system architecture and technical design decisions for effective team communication and knowledge sharing. | `skills/architecture-documenter.md` |
| `aspire-integration-testing` | Write integration tests using .NET Aspire's testing facilities with xUnit. Covers test fixtures, distributed application setup, endpoint discovery, and patterns for testing ASP.NET Core apps with real dependencies. | `skills/aspire-integration-testing.md` |
| `backend-patterns` | Backend architecture patterns, API design, database optimization, and server-side best practices for Node.js, Express, and Next.js API routes. | `skills/backend-patterns.md` |
| `bdd-dotnet` | Use this skill when writing unit tests for the Domain.Shared project. This skill captures the unique testing patterns, conventions, and philosophy used in this codebase. | `skills/bdd-dotnet.md` |
| `brain-memory` | Long-term project memory system. Persists context across sessions using brain.jsonl for tech stack, decisions, goals, errors, and compact summaries. | `skills/brain-memory.md` |
| `brainstorming` | You MUST use this before any creative work - creating features, building components, adding functionality, or modifying behavior. Explores user intent, requirements and design before implementation. | `skills/brainstorming.md` |
| `changelog-generator` | Automatically generate changelogs from git commits following conventional commits, semantic versioning, and best practices. | `skills/changelog-generator.md` |
| `chart-generator` | Generate charts and visualizations from data using various charting libraries and formats. | `skills/chart-generator.md` |
| `clickhouse-io` | ClickHouse database patterns, query optimization, analytics, and data engineering best practices for high-performance analytical workloads. | `skills/clickhouse-io.md` |
| `code-explainer` | Explain complex code to team members in clear, understandable terms for effective knowledge sharing and onboarding. | `skills/code-explainer.md` |
| `code-review` | Systematic code review framework for requesting and providing reviews. Use before merging PRs or when reviewing others' code. Ensures quality and knowledge sharing. | `skills/code-review.md` |
| `coding-standards` | Universal coding standards, best practices, and patterns for TypeScript, JavaScript, React, and Node.js development. | `skills/coding-standards.md` |
| `conflict-resolver` | Smart git merge conflict resolution with context analysis, pattern detection, and automated resolution strategies. | `skills/conflict-resolver.md` |
| `constitution` | Define project principles and non-negotiables before any implementation. Use at project start to establish governance, quality standards, and architectural constraints. | `skills/constitution.md` |
| `continuous-learning` | Automatically extract reusable patterns from Claude Code sessions and save them as learned skills for future use. | `skills/continuous-learning.md` |
| `csv-processor` | Parse, transform, and analyze CSV files with advanced data manipulation capabilities. | `skills/csv-processor.md` |
| `data-dotnet` | This skill provides a comprehensive guide to implementing the data persistence layer using Entity Framework Core as an adapter in a hexagonal architecture (ports and adapters pattern). The data layer is a **library that encapsulates its implementation internally** and **exposes DI registration publicly** for wiring into applications. | `skills/data-dotnet.md` |
| `data-validator` | Validate data against schemas, business rules, and data quality standards. | `skills/data-validator.md` |
| `ddd-dotnet` | This skill provides a comprehensive guide to implementing Domain-Driven Design patterns in .NET, based on real-world production code from the LMP project. These patterns promote clean architecture, maintainability, and testability. | `skills/ddd-dotnet.md` |
| `debugging` | Four-phase root cause analysis framework. Use when facing test failures, production bugs, unexpected behavior, or performance issues. Prevents symptom-fixing. | `skills/debugging.md` |
| `dispatching-parallel-agents` | Use when facing 2+ independent tasks that can be worked on without shared state or sequential dependencies | `skills/dispatching-parallel-agents.md` |
| `docker-infrastructure` | Use when starting/stopping Docker services, managing databases, running the fullstack application, or troubleshooting container issues. Provides commands for the shared infrastructure setup. | `skills/docker-infrastructure.md` |
| `dotnet-clean-architecture` | Use when creating .NET backend projects, implementing domain entities, CQRS handlers, or working with Clean Architecture patterns. References the clean-architecture-solution template. | `skills/dotnet-clean-architecture.md` |
| `epic-breakdown` | Organize features into hierarchical structure (Epics → Features → User Stories → Tasks). Use when planning large projects or organizing complex requirements. | `skills/epic-breakdown.md` |
| `eval-harness` | A formal evaluation framework for Claude Code sessions, implementing eval-driven development (EDD) principles. | `skills/eval-harness.md` |
| `executing-plans` | Use when you have a written implementation plan to execute in a separate session with review checkpoints | `skills/executing-plans.md` |
| `finishing-a-development-branch` | Use when implementation is complete, all tests pass, and you need to decide how to integrate the work - guides completion of development work by presenting structured options for merge, PR, or cleanup | `skills/finishing-a-development-branch.md` |
| `frontend-design` | Create distinctive, production-grade frontend interfaces with high design quality. Use when building web components, pages, or applications. Avoids generic AI aesthetics. | `skills/frontend-design.md` |
| `frontend-patterns` | Frontend development patterns for React, Next.js, state management, performance optimization, and UI best practices. | `skills/frontend-patterns.md` |
| `fullstack-development` | Use when building features that span both frontend and backend, or when setting up a new fullstack project. Orchestrates the clean-architecture-solution backend with react-frontend-template. | `skills/fullstack-development.md` |
| `git-worktrees-enhanced` | Create isolated git workspaces for feature development. Smart directory selection, safety verification, and cross-platform support (Windows/Unix). | `skills/git-worktrees-enhanced.md` |
| `go-mod-helper` | Go module system, dependency management, and project configuration assistance. | `skills/go-mod-helper.md` |
| `java-maven-helper` | Maven build system, dependency management, and Java project configuration assistance. | `skills/java-maven-helper.md` |
| `json-transformer` | Transform, manipulate, and analyze JSON data structures with advanced operations. | `skills/json-transformer.md` |
| `log-analyzer` | Parse and analyze application logs to identify errors, patterns, and insights. | `skills/log-analyzer.md` |
| `meeting-notes` | Convert meeting discussions into clear, actionable notes with tasks, decisions, and follow-ups for effective team collaboration. | `skills/meeting-notes.md` |
| `migration-generator` | Create database migrations from model changes, schema diffs, and migration best practices. | `skills/migration-generator.md` |
| `mock-server` | Create and manage mock API servers for development and testing. | `skills/mock-server.md` |
| `modern-csharp-coding-standards` | Write modern, high-performance C# code using records, pattern matching, value objects, async/await, Span<T>/Memory<T>, and best-practice API design patterns. Emphasizes functional-style programming with C# 12+ features. | `skills/modern-csharp-coding-standards.md` |
| `onboarding-helper` | Generate comprehensive onboarding documentation and guides for new developers joining your team or project. | `skills/onboarding-helper.md` |
| `openapi-generator` | Generate comprehensive OpenAPI/Swagger specifications from existing code and APIs. | `skills/openapi-generator.md` |
| `planning` | 3-file persistent planning pattern using markdown files. Use at start of complex tasks to maintain state across sessions. Treats filesystem as working memory. | `skills/planning.md` |
| `planning-with-files` | Implements Manus-style file-based planning for complex tasks. Creates task_plan.md, findings.md, and progress.md. Use when starting complex multi-step tasks, research projects, or any task requiring >5 tool calls. | `skills/planning-with-files.md` |
| `playwright-blazor-testing` | Write UI tests for Blazor applications (Server or WebAssembly) using Playwright. Covers navigation, interaction, authentication, selectors, and common Blazor-specific patterns. | `skills/playwright-blazor-testing.md` |
| `pr-template-generator` | Generate comprehensive pull request descriptions that help reviewers understand changes quickly and improve team collaboration. | `skills/pr-template-generator.md` |
| `project-from-idea` | Use when user wants to create a new project from scratch, has an idea for an app, or says "build me a..." - orchestrates the full journey from concept to working code using templates. | `skills/project-from-idea.md` |
| `project-guidelines-example` | This is an example of a project-specific skill. Use this as a template for your own projects. | `skills/project-guidelines-example.md` |
| `query-builder` | Interactive database query builder for generating optimized SQL and NoSQL queries. | `skills/query-builder.md` |
| `query-optimizer` | Analyze and optimize SQL queries for better performance and efficiency. | `skills/query-optimizer.md` |
| `quick-flow` | Lightweight track for simple changes - bug fixes, small features, config updates. Skip the full spec process when it's overkill. | `skills/quick-flow.md` |
| `ralph-wiggum` | Surgical Debugger & Code Optimizer. Autonomous root-cause investigation, persistence-loop fixing, and high-fidelity code reflection. No new features, only fixes. | `skills/ralph-wiggum.md` |
| `react-best-practices` | React and Next.js performance optimization guidelines from Vercel Engineering. This skill should be used when writing, reviewing, or refactoring React/Next.js code to ensure optimal performance patterns. Triggers on tasks involving React components, Next.js pages, data fetching, bundle optimization, or performance improvements. | `skills/react-best-practices.md` |
| `react-scaffold` | Use when creating React/Next.js frontend projects, adding React components, or implementing frontend features. References the react-frontend-template and best practices. | `skills/react-scaffold.md` |
| `receiving-code-review` | Use when receiving code review feedback, before implementing suggestions, especially if feedback seems unclear or technically questionable - requires technical rigor and verification, not performative agreement or blind implementation | `skills/receiving-code-review.md` |
| `requesting-code-review` | Use when completing tasks, implementing major features, or before merging to verify work meets requirements | `skills/requesting-code-review.md` |
| `resource-monitor` | Monitor system resources (CPU, memory, disk, network) during development and production. | `skills/resource-monitor.md` |
| `rn-animations` | React Native animations - Reanimated 3, gesture handling, micro-interactions, layout animations for mobile UX | `skills/rn-animations.md` |
| `rn-api-integration` | React Native API integration - Axios client setup, TanStack Query patterns, interceptors, error handling, offline support | `skills/rn-api-integration.md` |
| `rn-auth-integration` | React Native authentication - JWT auth flows, secure token storage, refresh token patterns, protected routes for Expo apps | `skills/rn-auth-integration.md` |
| `rn-crash-instrumentation` | React Native crash reporting - error boundaries, native crash capture, context attachment, Sentry advanced configuration | `skills/rn-crash-instrumentation.md` |
| `rn-deployment` | React Native deployment - EAS Build, App Store/Play Store submission, OTA updates, CI/CD pipelines for Expo apps | `skills/rn-deployment.md` |
| `rn-design-preset-system` | React Native design presets - switch between minimalist modern, glass aesthetic, and other curated design styles | `skills/rn-design-preset-system.md` |
| `rn-design-system-foundation` | React Native design system - design tokens, theming, component library foundation with minimalist modern preset | `skills/rn-design-system-foundation.md` |
| `rn-fundamentals` | React Native core concepts - components, styling, layout, Expo basics, TypeScript patterns for mobile development | `skills/rn-fundamentals.md` |
| `rn-native-modules` | React Native native modules - JSI, Turbo Modules, Fabric, native bridging for iOS (Swift/ObjC) and Android (Kotlin/Java) | `skills/rn-native-modules.md` |
| `rn-navigation` | React Native navigation patterns - Expo Router, React Navigation, deep linking, tab/stack/drawer navigation, authentication flows | `skills/rn-navigation.md` |
| `rn-observability-setup` | Mobile observability with Sentry - error tracking, performance monitoring, session replay, crash reporting, source maps for React Native/Expo | `skills/rn-observability-setup.md` |
| `rn-performance-monitoring` | React Native performance - screen load tracking, network tracing, app start measurement, performance budgets | `skills/rn-performance-monitoring.md` |
| `rn-state-management` | React Native state management - Redux Toolkit, TanStack Query, Zustand, AsyncStorage persistence, optimistic updates | `skills/rn-state-management.md` |
| `rn-testing` | React Native testing - Jest unit tests, React Native Testing Library, Detox E2E, Maestro flows for Expo apps | `skills/rn-testing.md` |
| `rust-cargo-assistant` | Cargo build system, crate management, and Rust project configuration assistance. | `skills/rust-cargo-assistant.md` |
| `schema-visualizer` | Generate database schema diagrams, ERDs, and documentation from database schemas. | `skills/schema-visualizer.md` |
| `search-enhancer` | Enhanced code search with semantic understanding, pattern matching, and intelligent query interpretation for faster code discovery. | `skills/search-enhancer.md` |
| `security` | Security-focused code analysis and audit skills. Use when reviewing code for vulnerabilities, performing security audits, or implementing security features. | `skills/security.md` |
| `seed-data-generator` | Generate realistic test data for database development, testing, and demos. | `skills/seed-data-generator.md` |
| `snippet-manager` | Save, organize, search, and retrieve code snippets with tags, categories, and smart search capabilities. | `skills/snippet-manager.md` |
| `spec-driven-development` | Use when starting a new project from an idea or vague requirements. Guides through systematic requirement gathering, specification creation, and user questions before any implementation. | `skills/spec-driven-development.md` |
| `strategic-compact` | Suggests manual context compaction at logical intervals to preserve context through task phases rather than arbitrary auto-compaction. | `skills/strategic-compact.md` |
| `subagent-driven-development` | Use when executing implementation plans with independent tasks in the current session | `skills/subagent-driven-development.md` |
| `systematic-debugging` | Use when encountering any bug, test failure, or unexpected behavior, before proposing fixes | `skills/systematic-debugging.md` |
| `tdd` | Enforce RED-GREEN-REFACTOR TDD cycle for all new features, bug fixes, and refactoring. Write tests first, watch them fail, implement minimal code to pass. | `skills/tdd.md` |
| `testcontainers-integration-tests` | Write integration tests using TestContainers for .NET with xUnit. Covers infrastructure testing with real databases, message queues, and caches in Docker containers instead of mocks. | `skills/testcontainers-integration-tests.md` |
| `tob-address-sanitizer` | > | `skills/tob-address-sanitizer.md` |
| `tob-aflpp` | > | `skills/tob-aflpp.md` |
| `tob-ask-questions-if-underspecified` | Clarify requirements before implementing. Use when serious doubts araise. | `skills/tob-ask-questions-if-underspecified.md` |
| `tob-atheris` | > | `skills/tob-atheris.md` |
| `tob-audit-context-building` | Enables ultra-granular, line-by-line code analysis to build deep architectural context before vulnerability or bug finding. | `skills/tob-audit-context-building.md` |
| `tob-cargo-fuzz` | > | `skills/tob-cargo-fuzz.md` |
| `tob-codeql` | > | `skills/tob-codeql.md` |
| `tob-constant-time-analysis` | Detects timing side-channel vulnerabilities in cryptographic code. Use when implementing or reviewing crypto code, encountering division on secrets, secret-dependent branches, or constant-time programming questions in C, C++, Go, Rust, Swift, Java, Kotlin, C#, PHP, JavaScript, TypeScript, Python, or Ruby. | `skills/tob-constant-time-analysis.md` |
| `tob-constant-time-testing` | > | `skills/tob-constant-time-testing.md` |
| `tob-coverage-analysis` | > | `skills/tob-coverage-analysis.md` |
| `tob-differential-review` | > | `skills/tob-differential-review.md` |
| `tob-dwarf-expert` | Provides expertise for analyzing DWARF debug files and understanding the DWARF debug format/standard (v3-v5). Triggers when understanding DWARF information, interacting with DWARF files, answering DWARF-related questions, or working with code that parses DWARF data. | `skills/tob-dwarf-expert.md` |
| `tob-entry-point-analyzer` | Analyzes smart contract codebases to identify state-changing entry points for security auditing. Detects externally callable functions that modify state, categorizes them by access level (public, admin, role-restricted, contract-only), and generates structured audit reports. Excludes view/pure/read-only functions. Use when auditing smart contracts (Solidity, Vyper, Solana/Rust, Move, TON, CosmWasm) or when asked to find entry points, audit flows, external functions, access control patterns, or privileged operations. | `skills/tob-entry-point-analyzer.md` |
| `tob-firebase-apk-scanner` | Scans Android APKs for Firebase security misconfigurations including open databases, storage buckets, authentication issues, and exposed cloud functions. Use when analyzing APK files for Firebase vulnerabilities, performing mobile app security audits, or testing Firebase endpoint security. For authorized security research only. | `skills/tob-firebase-apk-scanner.md` |
| `tob-fix-review` | > | `skills/tob-fix-review.md` |
| `tob-fuzzing-dictionary` | > | `skills/tob-fuzzing-dictionary.md` |
| `tob-fuzzing-obstacles` | > | `skills/tob-fuzzing-obstacles.md` |
| `tob-harness-writing` | > | `skills/tob-harness-writing.md` |
| `tob-interpreting-culture-index` | Use when interpreting Culture Index surveys, CI profiles, behavioral assessments, or personality data. Supports individual interpretation, team composition (gas/brake/glue), burnout detection, profile comparison, hiring profiles, manager coaching, interview transcript analysis for trait prediction, candidate debrief, onboarding planning, and conflict mediation. Handles PDF vision or JSON input. | `skills/tob-interpreting-culture-index.md` |
| `tob-libafl` | > | `skills/tob-libafl.md` |
| `tob-libfuzzer` | > | `skills/tob-libfuzzer.md` |
| `tob-ossfuzz` | > | `skills/tob-ossfuzz.md` |
| `tob-property-based-testing` | Provides guidance for property-based testing across multiple languages and smart contracts. Use when writing tests, reviewing code with serialization/validation/parsing patterns, designing features, or when property-based testing would provide stronger coverage than example-based tests. | `skills/tob-property-based-testing.md` |
| `tob-ruzzy` | > | `skills/tob-ruzzy.md` |
| `tob-sarif-parsing` | Parse, analyze, and process SARIF (Static Analysis Results Interchange Format) files. Use when reading security scan results, aggregating findings from multiple tools, deduplicating alerts, extracting specific vulnerabilities, or integrating SARIF data into CI/CD pipelines. | `skills/tob-sarif-parsing.md` |
| `tob-scripts` | Trail of Bits utility scripts collection - helper scripts for security testing workflows | `skills/tob-scripts.md` |
| `tob-semgrep` | > | `skills/tob-semgrep.md` |
| `tob-semgrep-rule-creator` | Create custom Semgrep rules for detecting bug patterns and security vulnerabilities. This skill should be used when the user explicitly asks to "create a Semgrep rule", "write a Semgrep rule", "make a Semgrep rule", "build a Semgrep rule", or requests detection of a specific bug pattern, vulnerability, or insecure code pattern using Semgrep. | `skills/tob-semgrep-rule-creator.md` |
| `tob-semgrep-rule-variant-creator` | Creates language variants of existing Semgrep rules. Use when porting a Semgrep rule to specified target languages. Takes an existing rule and target languages as input, produces independent rule+test directories for each language. | `skills/tob-semgrep-rule-variant-creator.md` |
| `tob-sharp-edges` | Identifies error-prone APIs, dangerous configurations, and footgun designs that enable security mistakes. Use when reviewing API designs, configuration schemas, cryptographic library ergonomics, or evaluating whether code follows 'secure by default' and 'pit of success' principles. Triggers: footgun, misuse-resistant, secure defaults, API usability, dangerous configuration. | `skills/tob-sharp-edges.md` |
| `tob-spec-to-code-compliance` | Verifies code implements exactly what documentation specifies for blockchain audits. Use when comparing code against whitepapers, finding gaps between specs and implementation, or performing compliance checks for protocol implementations. | `skills/tob-spec-to-code-compliance.md` |
| `tob-testing-handbook-generator` | > | `skills/tob-testing-handbook-generator.md` |
| `tob-variant-analysis` | Find similar vulnerabilities and bugs across codebases using pattern-based analysis. Use when hunting bug variants, building CodeQL/Semgrep queries, analyzing security vulnerabilities, or performing systematic code audits after finding an initial issue. | `skills/tob-variant-analysis.md` |
| `tob-wycheproof` | > | `skills/tob-wycheproof.md` |
| `using-git-worktrees` | Use when starting feature work that needs isolation from current workspace or before executing implementation plans - creates isolated git worktrees with smart directory selection and safety verification | `skills/using-git-worktrees.md` |
| `using-superpowers` | Use when starting any conversation - establishes how to find and use skills, requiring Skill tool invocation before ANY response including clarifying questions | `skills/using-superpowers.md` |
| `verification-before-completion` | Use when about to claim work is complete, fixed, or passing, before committing or creating PRs - requires running verification commands and confirming output before making any success claims; evidence before assertions always | `skills/verification-before-completion.md` |
| `verification-loop` | A comprehensive verification system for ensuring code quality before PRs. Runs build, type check, lint, tests, security scan, and diff review. | `skills/verification-loop.md` |
| `web-design-guidelines` | Review UI code for Web Interface Guidelines compliance. Use when asked to "review my UI", "check accessibility", "audit design", "review UX", or "check my site against best practices". | `skills/web-design-guidelines.md` |
| `webhook-tester` | Test webhook integrations locally with tunneling, inspection, and debugging tools. | `skills/webhook-tester.md` |
| `writing-plans` | Use when you have a spec or requirements for a multi-step task, before touching code | `skills/writing-plans.md` |
| `writing-skills` | Use when creating new skills, editing existing skills, or verifying skills work before deployment | `skills/writing-skills.md` |

---

## Skills by Category

### Development

- **code-review** (`skills/code-review.md`): Systematic code review framework for requesting and providing reviews. Use before merging PRs or when reviewing others' code. Ensures quality and knowledge sharing.
- **ralph-wiggum** (`skills/ralph-wiggum.md`): Surgical Debugger & Code Optimizer. Autonomous root-cause investigation, persistence-loop fixing, and high-fidelity code reflection. No new features, only fixes.

### Testing

- **akka-net-testing-patterns** (`skills/akka-net-testing-patterns.md`): Write unit and integration tests for Akka.NET actors using modern Akka.Hosting.TestKit patterns. Covers dependency injection, TestProbes, persistence testing, and actor interaction verification. Includes guidance on when to use traditional TestKit.
- **api-tester** (`skills/api-tester.md`): Quick API endpoint testing with comprehensive request/response validation.
- **aspire-integration-testing** (`skills/aspire-integration-testing.md`): Write integration tests using .NET Aspire's testing facilities with xUnit. Covers test fixtures, distributed application setup, endpoint discovery, and patterns for testing ASP.NET Core apps with real dependencies.
- **bdd-dotnet** (`skills/bdd-dotnet.md`): Use this skill when writing unit tests for the Domain.Shared project. This skill captures the unique testing patterns, conventions, and philosophy used in this codebase.
- **ddd-dotnet** (`skills/ddd-dotnet.md`): This skill provides a comprehensive guide to implementing Domain-Driven Design patterns in .NET, based on real-world production code from the LMP project. These patterns promote clean architecture, maintainability, and testability.
- **debugging** (`skills/debugging.md`): Four-phase root cause analysis framework. Use when facing test failures, production bugs, unexpected behavior, or performance issues. Prevents symptom-fixing.
- **finishing-a-development-branch** (`skills/finishing-a-development-branch.md`): Use when implementation is complete, all tests pass, and you need to decide how to integrate the work - guides completion of development work by presenting structured options for merge, PR, or cleanup
- **mock-server** (`skills/mock-server.md`): Create and manage mock API servers for development and testing.
- **playwright-blazor-testing** (`skills/playwright-blazor-testing.md`): Write UI tests for Blazor applications (Server or WebAssembly) using Playwright. Covers navigation, interaction, authentication, selectors, and common Blazor-specific patterns.
- **rn-testing** (`skills/rn-testing.md`): React Native testing - Jest unit tests, React Native Testing Library, Detox E2E, Maestro flows for Expo apps
- **seed-data-generator** (`skills/seed-data-generator.md`): Generate realistic test data for database development, testing, and demos.
- **systematic-debugging** (`skills/systematic-debugging.md`): Use when encountering any bug, test failure, or unexpected behavior, before proposing fixes
- **tdd** (`skills/tdd.md`): Enforce RED-GREEN-REFACTOR TDD cycle for all new features, bug fixes, and refactoring. Write tests first, watch them fail, implement minimal code to pass.
- **testcontainers-integration-tests** (`skills/testcontainers-integration-tests.md`): Write integration tests using TestContainers for .NET with xUnit. Covers infrastructure testing with real databases, message queues, and caches in Docker containers instead of mocks.
- **tob-constant-time-testing** (`skills/tob-constant-time-testing.md`): >
- **tob-firebase-apk-scanner** (`skills/tob-firebase-apk-scanner.md`): Scans Android APKs for Firebase security misconfigurations including open databases, storage buckets, authentication issues, and exposed cloud functions. Use when analyzing APK files for Firebase vulnerabilities, performing mobile app security audits, or testing Firebase endpoint security. For authorized security research only.
- **tob-property-based-testing** (`skills/tob-property-based-testing.md`): Provides guidance for property-based testing across multiple languages and smart contracts. Use when writing tests, reviewing code with serialization/validation/parsing patterns, designing features, or when property-based testing would provide stronger coverage than example-based tests.
- **tob-scripts** (`skills/tob-scripts.md`): Trail of Bits utility scripts collection - helper scripts for security testing workflows
- **tob-semgrep-rule-variant-creator** (`skills/tob-semgrep-rule-variant-creator.md`): Creates language variants of existing Semgrep rules. Use when porting a Semgrep rule to specified target languages. Takes an existing rule and target languages as input, produces independent rule+test directories for each language.
- **tob-testing-handbook-generator** (`skills/tob-testing-handbook-generator.md`): >
- **verification-loop** (`skills/verification-loop.md`): A comprehensive verification system for ensuring code quality before PRs. Runs build, type check, lint, tests, security scan, and diff review.
- **webhook-tester** (`skills/webhook-tester.md`): Test webhook integrations locally with tunneling, inspection, and debugging tools.

### Security

- **security** (`skills/security.md`): Security-focused code analysis and audit skills. Use when reviewing code for vulnerabilities, performing security audits, or implementing security features.
- **tob-address-sanitizer** (`skills/tob-address-sanitizer.md`): >
- **tob-aflpp** (`skills/tob-aflpp.md`): >
- **tob-ask-questions-if-underspecified** (`skills/tob-ask-questions-if-underspecified.md`): Clarify requirements before implementing. Use when serious doubts araise.
- **tob-atheris** (`skills/tob-atheris.md`): >
- **tob-audit-context-building** (`skills/tob-audit-context-building.md`): Enables ultra-granular, line-by-line code analysis to build deep architectural context before vulnerability or bug finding.
- **tob-cargo-fuzz** (`skills/tob-cargo-fuzz.md`): >
- **tob-codeql** (`skills/tob-codeql.md`): >
- **tob-constant-time-analysis** (`skills/tob-constant-time-analysis.md`): Detects timing side-channel vulnerabilities in cryptographic code. Use when implementing or reviewing crypto code, encountering division on secrets, secret-dependent branches, or constant-time programming questions in C, C++, Go, Rust, Swift, Java, Kotlin, C#, PHP, JavaScript, TypeScript, Python, or Ruby.
- **tob-coverage-analysis** (`skills/tob-coverage-analysis.md`): >
- **tob-differential-review** (`skills/tob-differential-review.md`): >
- **tob-dwarf-expert** (`skills/tob-dwarf-expert.md`): Provides expertise for analyzing DWARF debug files and understanding the DWARF debug format/standard (v3-v5). Triggers when understanding DWARF information, interacting with DWARF files, answering DWARF-related questions, or working with code that parses DWARF data.
- **tob-entry-point-analyzer** (`skills/tob-entry-point-analyzer.md`): Analyzes smart contract codebases to identify state-changing entry points for security auditing. Detects externally callable functions that modify state, categorizes them by access level (public, admin, role-restricted, contract-only), and generates structured audit reports. Excludes view/pure/read-only functions. Use when auditing smart contracts (Solidity, Vyper, Solana/Rust, Move, TON, CosmWasm) or when asked to find entry points, audit flows, external functions, access control patterns, or privileged operations.
- **tob-fix-review** (`skills/tob-fix-review.md`): >
- **tob-fuzzing-dictionary** (`skills/tob-fuzzing-dictionary.md`): >
- **tob-fuzzing-obstacles** (`skills/tob-fuzzing-obstacles.md`): >
- **tob-harness-writing** (`skills/tob-harness-writing.md`): >
- **tob-interpreting-culture-index** (`skills/tob-interpreting-culture-index.md`): Use when interpreting Culture Index surveys, CI profiles, behavioral assessments, or personality data. Supports individual interpretation, team composition (gas/brake/glue), burnout detection, profile comparison, hiring profiles, manager coaching, interview transcript analysis for trait prediction, candidate debrief, onboarding planning, and conflict mediation. Handles PDF vision or JSON input.
- **tob-libafl** (`skills/tob-libafl.md`): >
- **tob-libfuzzer** (`skills/tob-libfuzzer.md`): >
- **tob-ossfuzz** (`skills/tob-ossfuzz.md`): >
- **tob-ruzzy** (`skills/tob-ruzzy.md`): >
- **tob-sarif-parsing** (`skills/tob-sarif-parsing.md`): Parse, analyze, and process SARIF (Static Analysis Results Interchange Format) files. Use when reading security scan results, aggregating findings from multiple tools, deduplicating alerts, extracting specific vulnerabilities, or integrating SARIF data into CI/CD pipelines.
- **tob-semgrep** (`skills/tob-semgrep.md`): >
- **tob-semgrep-rule-creator** (`skills/tob-semgrep-rule-creator.md`): Create custom Semgrep rules for detecting bug patterns and security vulnerabilities. This skill should be used when the user explicitly asks to "create a Semgrep rule", "write a Semgrep rule", "make a Semgrep rule", "build a Semgrep rule", or requests detection of a specific bug pattern, vulnerability, or insecure code pattern using Semgrep.
- **tob-sharp-edges** (`skills/tob-sharp-edges.md`): Identifies error-prone APIs, dangerous configurations, and footgun designs that enable security mistakes. Use when reviewing API designs, configuration schemas, cryptographic library ergonomics, or evaluating whether code follows 'secure by default' and 'pit of success' principles. Triggers: footgun, misuse-resistant, secure defaults, API usability, dangerous configuration.
- **tob-spec-to-code-compliance** (`skills/tob-spec-to-code-compliance.md`): Verifies code implements exactly what documentation specifies for blockchain audits. Use when comparing code against whitepapers, finding gaps between specs and implementation, or performing compliance checks for protocol implementations.
- **tob-variant-analysis** (`skills/tob-variant-analysis.md`): Find similar vulnerabilities and bugs across codebases using pattern-based analysis. Use when hunting bug variants, building CodeQL/Semgrep queries, analyzing security vulnerabilities, or performing systematic code audits after finding an initial issue.
- **tob-wycheproof** (`skills/tob-wycheproof.md`): >
- **web-design-guidelines** (`skills/web-design-guidelines.md`): Review UI code for Web Interface Guidelines compliance. Use when asked to "review my UI", "check accessibility", "audit design", "review UX", or "check my site against best practices".

### Documentation

- **api-documentation** (`skills/api-documentation.md`): Auto-generate comprehensive API documentation with examples, schemas, and interactive tools.
- **architecture-documenter** (`skills/architecture-documenter.md`): Document system architecture and technical design decisions for effective team communication and knowledge sharing.
- **changelog-generator** (`skills/changelog-generator.md`): Automatically generate changelogs from git commits following conventional commits, semantic versioning, and best practices.
- **docker-infrastructure** (`skills/docker-infrastructure.md`): Use when starting/stopping Docker services, managing databases, running the fullstack application, or troubleshooting container issues. Provides commands for the shared infrastructure setup.
- **onboarding-helper** (`skills/onboarding-helper.md`): Generate comprehensive onboarding documentation and guides for new developers joining your team or project.
- **schema-visualizer** (`skills/schema-visualizer.md`): Generate database schema diagrams, ERDs, and documentation from database schemas.

### Planning

- **brainstorming** (`skills/brainstorming.md`): You MUST use this before any creative work - creating features, building components, adding functionality, or modifying behavior. Explores user intent, requirements and design before implementation.
- **epic-breakdown** (`skills/epic-breakdown.md`): Organize features into hierarchical structure (Epics → Features → User Stories → Tasks). Use when planning large projects or organizing complex requirements.
- **executing-plans** (`skills/executing-plans.md`): Use when you have a written implementation plan to execute in a separate session with review checkpoints
- **openapi-generator** (`skills/openapi-generator.md`): Generate comprehensive OpenAPI/Swagger specifications from existing code and APIs.
- **planning** (`skills/planning.md`): 3-file persistent planning pattern using markdown files. Use at start of complex tasks to maintain state across sessions. Treats filesystem as working memory.
- **planning-with-files** (`skills/planning-with-files.md`): Implements Manus-style file-based planning for complex tasks. Creates task_plan.md, findings.md, and progress.md. Use when starting complex multi-step tasks, research projects, or any task requiring >5 tool calls.
- **project-guidelines-example** (`skills/project-guidelines-example.md`): This is an example of a project-specific skill. Use this as a template for your own projects.
- **quick-flow** (`skills/quick-flow.md`): Lightweight track for simple changes - bug fixes, small features, config updates. Skip the full spec process when it's overkill.
- **receiving-code-review** (`skills/receiving-code-review.md`): Use when receiving code review feedback, before implementing suggestions, especially if feedback seems unclear or technically questionable - requires technical rigor and verification, not performative agreement or blind implementation
- **spec-driven-development** (`skills/spec-driven-development.md`): Use when starting a new project from an idea or vague requirements. Guides through systematic requirement gathering, specification creation, and user questions before any implementation.
- **subagent-driven-development** (`skills/subagent-driven-development.md`): Use when executing implementation plans with independent tasks in the current session
- **using-git-worktrees** (`skills/using-git-worktrees.md`): Use when starting feature work that needs isolation from current workspace or before executing implementation plans - creates isolated git worktrees with smart directory selection and safety verification
- **writing-plans** (`skills/writing-plans.md`): Use when you have a spec or requirements for a multi-step task, before touching code

### Mobile

- **data-dotnet** (`skills/data-dotnet.md`): This skill provides a comprehensive guide to implementing the data persistence layer using Entity Framework Core as an adapter in a hexagonal architecture (ports and adapters pattern). The data layer is a **library that encapsulates its implementation internally** and **exposes DI registration publicly** for wiring into applications.
- **modern-csharp-coding-standards** (`skills/modern-csharp-coding-standards.md`): Write modern, high-performance C# code using records, pattern matching, value objects, async/await, Span<T>/Memory<T>, and best-practice API design patterns. Emphasizes functional-style programming with C# 12+ features.
- **rn-animations** (`skills/rn-animations.md`): React Native animations - Reanimated 3, gesture handling, micro-interactions, layout animations for mobile UX
- **rn-api-integration** (`skills/rn-api-integration.md`): React Native API integration - Axios client setup, TanStack Query patterns, interceptors, error handling, offline support
- **rn-auth-integration** (`skills/rn-auth-integration.md`): React Native authentication - JWT auth flows, secure token storage, refresh token patterns, protected routes for Expo apps
- **rn-crash-instrumentation** (`skills/rn-crash-instrumentation.md`): React Native crash reporting - error boundaries, native crash capture, context attachment, Sentry advanced configuration
- **rn-deployment** (`skills/rn-deployment.md`): React Native deployment - EAS Build, App Store/Play Store submission, OTA updates, CI/CD pipelines for Expo apps
- **rn-design-preset-system** (`skills/rn-design-preset-system.md`): React Native design presets - switch between minimalist modern, glass aesthetic, and other curated design styles
- **rn-design-system-foundation** (`skills/rn-design-system-foundation.md`): React Native design system - design tokens, theming, component library foundation with minimalist modern preset
- **rn-fundamentals** (`skills/rn-fundamentals.md`): React Native core concepts - components, styling, layout, Expo basics, TypeScript patterns for mobile development
- **rn-native-modules** (`skills/rn-native-modules.md`): React Native native modules - JSI, Turbo Modules, Fabric, native bridging for iOS (Swift/ObjC) and Android (Kotlin/Java)
- **rn-navigation** (`skills/rn-navigation.md`): React Native navigation patterns - Expo Router, React Navigation, deep linking, tab/stack/drawer navigation, authentication flows
- **rn-observability-setup** (`skills/rn-observability-setup.md`): Mobile observability with Sentry - error tracking, performance monitoring, session replay, crash reporting, source maps for React Native/Expo
- **rn-performance-monitoring** (`skills/rn-performance-monitoring.md`): React Native performance - screen load tracking, network tracing, app start measurement, performance budgets
- **rn-state-management** (`skills/rn-state-management.md`): React Native state management - Redux Toolkit, TanStack Query, Zustand, AsyncStorage persistence, optimistic updates

### Backend

- **akka-net-aspire-configuration** (`skills/akka-net-aspire-configuration.md`): Configure Akka.NET with .NET Aspire for local development and production deployments. Covers actor system setup, clustering, persistence, Akka.Management integration, and Aspire orchestration patterns.
- **backend-patterns** (`skills/backend-patterns.md`): Backend architecture patterns, API design, database optimization, and server-side best practices for Node.js, Express, and Next.js API routes.
- **dotnet-clean-architecture** (`skills/dotnet-clean-architecture.md`): Use when creating .NET backend projects, implementing domain entities, CQRS handlers, or working with Clean Architecture patterns. References the clean-architecture-solution template.
- **fullstack-development** (`skills/fullstack-development.md`): Use when building features that span both frontend and backend, or when setting up a new fullstack project. Orchestrates the clean-architecture-solution backend with react-frontend-template.

### Frontend

- **coding-standards** (`skills/coding-standards.md`): Universal coding standards, best practices, and patterns for TypeScript, JavaScript, React, and Node.js development.
- **frontend-design** (`skills/frontend-design.md`): Create distinctive, production-grade frontend interfaces with high design quality. Use when building web components, pages, or applications. Avoids generic AI aesthetics.
- **frontend-patterns** (`skills/frontend-patterns.md`): Frontend development patterns for React, Next.js, state management, performance optimization, and UI best practices.
- **java-maven-helper** (`skills/java-maven-helper.md`): Maven build system, dependency management, and Java project configuration assistance.
- **pr-template-generator** (`skills/pr-template-generator.md`): Generate comprehensive pull request descriptions that help reviewers understand changes quickly and improve team collaboration.
- **project-from-idea** (`skills/project-from-idea.md`): Use when user wants to create a new project from scratch, has an idea for an app, or says "build me a..." - orchestrates the full journey from concept to working code using templates.
- **query-builder** (`skills/query-builder.md`): Interactive database query builder for generating optimized SQL and NoSQL queries.
- **react-best-practices** (`skills/react-best-practices.md`): React and Next.js performance optimization guidelines from Vercel Engineering. This skill should be used when writing, reviewing, or refactoring React/Next.js code to ensure optimal performance patterns. Triggers on tasks involving React components, Next.js pages, data fetching, bundle optimization, or performance improvements.
- **react-scaffold** (`skills/react-scaffold.md`): Use when creating React/Next.js frontend projects, adding React components, or implementing frontend features. References the react-frontend-template and best practices.
- **requesting-code-review** (`skills/requesting-code-review.md`): Use when completing tasks, implementing major features, or before merging to verify work meets requirements
- **rust-cargo-assistant** (`skills/rust-cargo-assistant.md`): Cargo build system, crate management, and Rust project configuration assistance.
- **using-superpowers** (`skills/using-superpowers.md`): Use when starting any conversation - establishes how to find and use skills, requiring Skill tool invocation before ANY response including clarifying questions
- **verification-before-completion** (`skills/verification-before-completion.md`): Use when about to claim work is complete, fixed, or passing, before committing or creating PRs - requires running verification commands and confirming output before making any success claims; evidence before assertions always

### DevOps

- **brain-memory** (`skills/brain-memory.md`): Long-term project memory system. Persists context across sessions using brain.jsonl for tech stack, decisions, goals, errors, and compact summaries.
- **conflict-resolver** (`skills/conflict-resolver.md`): Smart git merge conflict resolution with context analysis, pattern detection, and automated resolution strategies.
- **constitution** (`skills/constitution.md`): Define project principles and non-negotiables before any implementation. Use at project start to establish governance, quality standards, and architectural constraints.
- **dispatching-parallel-agents** (`skills/dispatching-parallel-agents.md`): Use when facing 2+ independent tasks that can be worked on without shared state or sequential dependencies
- **eval-harness** (`skills/eval-harness.md`): A formal evaluation framework for Claude Code sessions, implementing eval-driven development (EDD) principles.
- **git-worktrees-enhanced** (`skills/git-worktrees-enhanced.md`): Create isolated git workspaces for feature development. Smart directory selection, safety verification, and cross-platform support (Windows/Unix).
- **meeting-notes** (`skills/meeting-notes.md`): Convert meeting discussions into clear, actionable notes with tasks, decisions, and follow-ups for effective team collaboration.
- **query-optimizer** (`skills/query-optimizer.md`): Analyze and optimize SQL queries for better performance and efficiency.
- **writing-skills** (`skills/writing-skills.md`): Use when creating new skills, editing existing skills, or verifying skills work before deployment

### General

- **add-serena** (`skills/add-serena.md`): This skill helps you add Serena MCP (Model Context Protocol) to any software project, providing IDE-like semantic code understanding and navigation capabilities with true multi-project support.
- **chart-generator** (`skills/chart-generator.md`): Generate charts and visualizations from data using various charting libraries and formats.
- **clickhouse-io** (`skills/clickhouse-io.md`): ClickHouse database patterns, query optimization, analytics, and data engineering best practices for high-performance analytical workloads.
- **code-explainer** (`skills/code-explainer.md`): Explain complex code to team members in clear, understandable terms for effective knowledge sharing and onboarding.
- **continuous-learning** (`skills/continuous-learning.md`): Automatically extract reusable patterns from Claude Code sessions and save them as learned skills for future use.
- **csv-processor** (`skills/csv-processor.md`): Parse, transform, and analyze CSV files with advanced data manipulation capabilities.
- **data-validator** (`skills/data-validator.md`): Validate data against schemas, business rules, and data quality standards.
- **go-mod-helper** (`skills/go-mod-helper.md`): Go module system, dependency management, and project configuration assistance.
- **json-transformer** (`skills/json-transformer.md`): Transform, manipulate, and analyze JSON data structures with advanced operations.
- **log-analyzer** (`skills/log-analyzer.md`): Parse and analyze application logs to identify errors, patterns, and insights.
- **migration-generator** (`skills/migration-generator.md`): Create database migrations from model changes, schema diffs, and migration best practices.
- **resource-monitor** (`skills/resource-monitor.md`): Monitor system resources (CPU, memory, disk, network) during development and production.
- **search-enhancer** (`skills/search-enhancer.md`): Enhanced code search with semantic understanding, pattern matching, and intelligent query interpretation for faster code discovery.
- **snippet-manager** (`skills/snippet-manager.md`): Save, organize, search, and retrieve code snippets with tags, categories, and smart search capabilities.
- **strategic-compact** (`skills/strategic-compact.md`): Suggests manual context compaction at logical intervals to preserve context through task phases rather than arbitrary auto-compaction.

---

*Generated by ai-config-sync • 129 skills*
