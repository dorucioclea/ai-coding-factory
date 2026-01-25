# /security-review - Perform Security Review

Conduct a security review of code changes or the entire codebase.

## Usage
```
/security-review [scope]
```

Where scope is:
- `staged` (default) - Review staged git changes
- `recent` - Review last N commits (default 5)
- `file <path>` - Review specific file
- `full` - Full codebase security audit

## Instructions

### Security Review Checklist

For each scope, check for:

#### 1. Secrets Detection
- Hardcoded passwords, API keys, tokens
- Private keys or certificates
- Connection strings with credentials
- AWS/Azure/GCP credentials

```bash
# Patterns to search for
grep -rn "password\s*=" --include="*.cs" --include="*.json"
grep -rn "apikey\|api_key\|api-key" --include="*.cs" --include="*.json"
grep -rn "secret\s*=" --include="*.cs" --include="*.json"
grep -rn "connectionstring\|connection_string" --include="*.cs" --include="*.json"
```

#### 2. SQL Injection Vulnerabilities
- Raw SQL queries with string concatenation
- Non-parameterized queries
- Dynamic SQL construction

```csharp
// BAD: Vulnerable to SQL injection
var query = $"SELECT * FROM Users WHERE Id = {userId}";

// GOOD: Parameterized query
var user = await context.Users.FindAsync(userId);
```

#### 3. XSS Vulnerabilities
- Unencoded user input in responses
- Raw HTML rendering
- Missing Content-Security-Policy headers

#### 4. Authentication/Authorization
- Missing [Authorize] attributes
- Improper role checks
- JWT configuration issues
- Session management problems

#### 5. Input Validation
- Missing FluentValidation rules
- Unvalidated file uploads
- Missing size limits
- Path traversal vulnerabilities

#### 6. Dependency Vulnerabilities
```bash
dotnet list package --vulnerable
```

#### 7. Configuration Security
- Debug mode in production
- Verbose error messages
- Missing HTTPS enforcement
- Weak CORS policies

### Output Format

```markdown
# Security Review Report
Date: <timestamp>
Scope: <scope>
Reviewer: Claude Code

## Summary
| Severity | Count |
|----------|-------|
| Critical | X |
| High | X |
| Medium | X |
| Low | X |
| Info | X |

## Findings

### Critical

#### [CRIT-001] Hardcoded API Key
**File**: src/Services/PaymentService.cs:45
**Issue**: API key hardcoded in source code
**Risk**: Credential exposure in version control
**Remediation**: Move to environment variable or secret store

```csharp
// Current (vulnerable)
var apiKey = "sk_live_abc123";

// Recommended
var apiKey = Environment.GetEnvironmentVariable("PAYMENT_API_KEY");
```

---

### High

#### [HIGH-001] SQL Injection Risk
**File**: src/Repositories/UserRepository.cs:78
**Issue**: String concatenation in SQL query
...

---

## Recommendations

1. Immediate action required for Critical findings
2. Address High findings before release
3. Plan remediation for Medium findings
4. Consider addressing Low/Info in next sprint

## Security Checklist Status

- [ ] No hardcoded secrets
- [ ] All queries parameterized
- [ ] Input validation on all endpoints
- [ ] Authentication on protected routes
- [ ] HTTPS enforced
- [ ] Security headers configured
- [ ] Dependencies updated
- [ ] Error messages sanitized
```

## Severity Guidelines

- **Critical**: Immediate exploitation possible, data breach risk
- **High**: Significant security weakness, requires prompt attention
- **Medium**: Security best practice violation, should be addressed
- **Low**: Minor issues, defense in depth improvements
- **Info**: Recommendations for hardening

## Example

```
User: /security-review staged

Claude: Reviewing staged changes for security issues...

## Security Review Results

### Files Reviewed
- src/Controllers/UserController.cs
- src/Services/AuthService.cs

### Findings

| Severity | Count |
|----------|-------|
| Critical | 0 |
| High | 1 |
| Medium | 2 |
| Low | 0 |

#### [HIGH-001] Missing Authorization
**File**: src/Controllers/UserController.cs:34
**Issue**: DELETE endpoint missing [Authorize] attribute
**Risk**: Unauthenticated users could delete records

Recommendation: Add `[Authorize(Roles = "Admin")]` to the endpoint.

---

All critical issues: 0
Ready for commit: Yes (after addressing HIGH finding)
```
