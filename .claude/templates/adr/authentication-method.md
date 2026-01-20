# ADR-{{NUMBER}}: Authentication Method Selection

## Status
Proposed | Accepted | Deprecated | Superseded

## Date
{{DATE}}

## Story
{{STORY_ID}}

## Context

We need to implement authentication for {{PROJECT_NAME}}. Requirements include:

- **User types**: [Internal | External | Both | Machine-to-machine]
- **Identity source**: [Self-managed | External IdP | Federated]
- **Session duration**: [Short-lived | Long-lived | Configurable]
- **Multi-factor**: [Required | Optional | Not needed]
- **Mobile support**: [Yes | No]

### Current State
<!-- Describe current authentication if any -->

### Security Requirements
- Token expiration: {{EXPIRATION}}
- Refresh token support: [Yes | No]
- Token revocation: [Required | Nice-to-have]
- Audit logging: [Required | Optional]

## Options Considered

### Option 1: JWT Bearer Tokens
**Type**: Stateless token-based

**Pros**:
- Stateless, scales horizontally
- Self-contained with claims
- Standard (RFC 7519)
- Works well with microservices
- No server-side session storage

**Cons**:
- Cannot revoke individual tokens (without blacklist)
- Token size can grow with claims
- Must handle token refresh carefully

**Implementation**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]))
        };
    });
```

### Option 2: OAuth 2.0 / OpenID Connect with External IdP
**Type**: Delegated authentication

**Providers**: Azure AD, Auth0, Okta, Keycloak

**Pros**:
- Offloads identity management
- Enterprise SSO support
- MFA built-in
- Compliance certifications
- Social login support

**Cons**:
- External dependency
- Cost for commercial providers
- More complex setup
- Network dependency

**Implementation**:
```csharp
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));
```

### Option 3: Cookie-based Sessions
**Type**: Stateful server-side

**Pros**:
- Simple to implement
- Easy to revoke (delete session)
- Browser handles automatically
- Works without JavaScript

**Cons**:
- Requires session storage
- CSRF protection needed
- Not ideal for APIs
- Scaling requires distributed cache

**Implementation**:
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
```

### Option 4: API Keys
**Type**: Static credentials

**Pros**:
- Simple for machine-to-machine
- No token refresh needed
- Easy to implement

**Cons**:
- No built-in expiration
- Manual rotation required
- Not suitable for user authentication
- Must be transmitted securely

**Use case**: Service-to-service, webhooks, third-party integrations

## Decision

We will use **{{SELECTED_OPTION}}** because:

1. {{REASON_1}}
2. {{REASON_2}}
3. {{REASON_3}}

### Hybrid Approach (if applicable)
- User authentication: {{USER_AUTH_METHOD}}
- Service-to-service: {{SERVICE_AUTH_METHOD}}

## Consequences

### Positive
- {{POSITIVE_1}}
- {{POSITIVE_2}}

### Negative
- {{NEGATIVE_1}}
- {{NEGATIVE_2}}

### Security Considerations
- Token storage: {{TOKEN_STORAGE}}
- Secret management: {{SECRET_MANAGEMENT}}
- HTTPS enforcement: Required

## Implementation Notes

### Token Lifetime Configuration
```json
{
  "Jwt": {
    "AccessTokenExpiry": "00:15:00",
    "RefreshTokenExpiry": "7.00:00:00"
  }
}
```

### Required Endpoints
- `POST /api/auth/login` - Authenticate and get tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Revoke refresh token
- `GET /api/auth/me` - Get current user

### Password Requirements (if self-managed)
- Minimum length: 12 characters
- Complexity: Upper, lower, number, special
- Hashing: BCrypt or Argon2

## References
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
- [OAuth 2.0 Security Best Practices](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
