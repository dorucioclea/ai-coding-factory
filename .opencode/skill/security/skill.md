---
name: security-analysis
description: Security-focused code analysis and audit skills. Use when reviewing code for vulnerabilities, performing security audits, or implementing security features.
license: MIT
compatibility: opencode
metadata:
  audience: developers
  category: security
---

# Security Analysis

## Core Principle

> **Security is not a feature—it's a requirement.**

Every code change must be evaluated for security implications.

## When to Use This Skill

- Code reviews (especially auth, input handling, data access)
- New feature implementation
- Dependency updates
- Before releases
- Incident investigation

## OWASP Top 10 Checklist

### 1. Injection (SQL, Command, LDAP)

**Check for:**
```csharp
// ❌ BAD - SQL Injection
var query = $"SELECT * FROM Users WHERE Id = {userId}";

// ✅ GOOD - Parameterized
var user = await context.Users.FindAsync(userId);
```

**Rules:**
- Never concatenate user input into queries
- Use parameterized queries or ORM
- Validate and sanitize all inputs

### 2. Broken Authentication

**Check for:**
- Weak password policies
- Missing rate limiting
- Session fixation vulnerabilities
- Insecure "remember me" functionality

```csharp
// ✅ GOOD - Proper password hashing
services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
});
```

### 3. Sensitive Data Exposure

**Check for:**
- Hardcoded secrets
- Unencrypted sensitive data
- Overly verbose error messages
- Logging of sensitive information

```csharp
// ❌ BAD - Hardcoded secret
var apiKey = "sk-1234567890abcdef";

// ✅ GOOD - From configuration
var apiKey = configuration["ApiKeys:ExternalService"];
```

### 4. XML External Entities (XXE)

**Check for:**
- XML parsing with external entity processing enabled
- Unvalidated XML input

```csharp
// ✅ GOOD - Disable DTD processing
var settings = new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver = null
};
```

### 5. Broken Access Control

**Check for:**
- Missing authorization checks
- Insecure direct object references
- Path traversal vulnerabilities

```csharp
// ❌ BAD - No ownership check
public async Task<Order> GetOrder(int orderId)
{
    return await context.Orders.FindAsync(orderId);
}

// ✅ GOOD - Verify ownership
public async Task<Order> GetOrder(int orderId, int userId)
{
    return await context.Orders
        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
}
```

### 6. Security Misconfiguration

**Check for:**
- Default credentials
- Unnecessary features enabled
- Missing security headers
- Verbose error messages in production

```csharp
// ✅ GOOD - Security headers
app.UseHsts();
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

### 7. Cross-Site Scripting (XSS)

**Check for:**
- Unencoded user input in HTML
- Dangerous innerHTML usage
- Reflected input in responses

```tsx
// ❌ BAD - XSS vulnerability
<div dangerouslySetInnerHTML={{ __html: userInput }} />

// ✅ GOOD - Encoded output
<div>{userInput}</div>
```

### 8. Insecure Deserialization

**Check for:**
- Deserializing untrusted data
- Type-based deserialization vulnerabilities

```csharp
// ❌ BAD - Dangerous deserialization
var obj = JsonConvert.DeserializeObject(userInput,
    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

// ✅ GOOD - Explicit types
var obj = JsonSerializer.Deserialize<MyDto>(userInput);
```

### 9. Using Components with Known Vulnerabilities

**Check for:**
- Outdated NuGet packages
- Outdated npm packages
- Known CVEs in dependencies

```bash
# Check .NET vulnerabilities
dotnet list package --vulnerable

# Check npm vulnerabilities
npm audit
```

### 10. Insufficient Logging & Monitoring

**Check for:**
- Missing audit logs for security events
- No alerting on suspicious activity
- Logs containing sensitive data

```csharp
// ✅ GOOD - Security event logging
_logger.LogWarning("Failed login attempt for user {UserId} from IP {IpAddress}",
    userId, ipAddress);
```

## React/Frontend Security

### Content Security Policy
```tsx
// next.config.js
const securityHeaders = [
  {
    key: 'Content-Security-Policy',
    value: "default-src 'self'; script-src 'self' 'unsafe-inline';"
  }
];
```

### Secure State Management
- Never store sensitive data in localStorage
- Use httpOnly cookies for tokens
- Clear sensitive state on logout

### Input Validation
```tsx
import { z } from 'zod';

const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(12)
});
```

## Security Review Checklist

### Before PR Merge
- [ ] No hardcoded secrets
- [ ] All inputs validated
- [ ] Authorization checks present
- [ ] Sensitive data encrypted
- [ ] No SQL/command injection
- [ ] XSS prevention in place
- [ ] Dependencies scanned
- [ ] Security headers configured
- [ ] Error messages don't leak info
- [ ] Audit logging for security events

## Integration with AI Coding Factory

### Hooks
- `pre-release-checklist.sh` - Runs security scan
- `post-security-scan.sh` - Archives and tracks results

### Artifacts
- Store scan results in `artifacts/security/`
- Track baseline and trends

### Story Linkage
- Security requirements in acceptance criteria
- Security tests linked to story: `[Trait("Story", "ACF-###")]`
