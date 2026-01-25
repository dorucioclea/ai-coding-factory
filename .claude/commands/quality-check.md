---
description: Run comprehensive code quality checks combining multiple analysis tools
---

Run a comprehensive code quality check combining multiple skills and validation tools.

## Quality Check Workflow

Perform the following checks in order based on project type:

### 1. Code Quality Analysis

- **Code Review** - Use code-reviewer agent for code quality analysis
- **Pattern Detection** - Use pattern-detector skill to identify anti-patterns
- **Complexity Analysis** - Use complexity-analyzer skill to measure cyclomatic complexity
- **Dead Code Detection** - Use dead-code-detector or refactor-cleaner agent

### 2. Security Analysis

- **Secret Scanner** - Check for exposed secrets in codebase
- **Dependency Audit** - Check for vulnerable dependencies
- **Security Headers** - Validate security headers (web apps)

### 3. .NET Specific (if applicable)

- Run `dotnet build --warningsAsErrors`
- Run `dotnet test --collect:"XPlat Code Coverage"`
- Check coverage meets 80% threshold
- Run architecture tests with NetArchTest

### 4. Frontend Specific (if applicable)

- Run `npm run lint` or `pnpm lint`
- Run `npm run build` to check for TypeScript errors
- Run `npm test -- --coverage`

## Instructions

For each applicable check:
1. Run the analysis
2. Report findings
3. Provide actionable recommendations
4. Note severity (Critical/High/Medium/Low)

## Final Summary Report

Generate a final summary with:
- **Overall Quality Score** (0-100)
- **Critical Issues** (must fix immediately)
- **High Issues** (fix before merge)
- **Medium Issues** (fix when possible)
- **Top 5 Recommendations**
- **Quick Wins** (easy fixes with high impact)

## Integration with CI

This check aligns with the PR validation pipeline:
```bash
./scripts/validate-project.sh
./scripts/validate-documentation.sh
```
