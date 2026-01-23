# Security Guidelines

## Security Principles

1. **Defense in Depth** - Multiple layers of security controls
2. **Least Privilege** - Minimum necessary permissions
3. **Fail Secure** - Default to deny on errors
4. **Input Validation** - Never trust user input
5. **Secure by Default** - Security enabled out of the box

## OWASP Top 10 Mitigations

### 1. Injection Prevention

```csharp
// ✅ GOOD: Parameterized queries with EF Core
var orders = await context.Orders
    .Where(o => o.CustomerId == customerId)
    .ToListAsync();

// ❌ BAD: String concatenation (SQL Injection)
var query = $"SELECT * FROM Orders WHERE CustomerId = '{customerId}'";

// ✅ GOOD: Parameterized raw SQL when needed
var orders = await context.Orders
    .FromSqlInterpolated($"SELECT * FROM Orders WHERE CustomerId = {customerId}")
    .ToListAsync();
```

### 2. Authentication & Session Management

```csharp
// JWT Configuration
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
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero // No tolerance for token expiry
        };
    });

// Token Generation
public string GenerateToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1), // Short-lived tokens
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### 3. Sensitive Data Exposure

```csharp
// ✅ GOOD: Use IConfiguration for secrets
public class EmailService(IConfiguration config)
{
    private readonly string _apiKey = config["SendGrid:ApiKey"]!;
}

// ❌ BAD: Hardcoded secrets
public class EmailService
{
    private const string ApiKey = "SG.xxx"; // NEVER DO THIS
}

// ✅ GOOD: Exclude sensitive data from logs
_logger.LogInformation("User {UserId} logged in", user.Id);

// ❌ BAD: Logging sensitive data
_logger.LogInformation("User logged in with password {Password}", password);
```

### 4. XML External Entities (XXE)

```csharp
// ✅ GOOD: Secure XML parsing
var settings = new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver = null
};
using var reader = XmlReader.Create(stream, settings);

// ❌ BAD: Default XML parsing
var doc = new XmlDocument();
doc.Load(stream); // Vulnerable to XXE
```

### 5. Access Control

```csharp
// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("OrderOwner", policy =>
        policy.AddRequirements(new OrderOwnerRequirement()));
});

// Resource-based authorization
public class OrderOwnerHandler : AuthorizationHandler<OrderOwnerRequirement, Order>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrderOwnerRequirement requirement,
        Order order)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (order.CustomerId.ToString() == userId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

// Controller usage
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order is null) return NotFound();

        var authResult = await _authService.AuthorizeAsync(User, order, "OrderOwner");
        if (!authResult.Succeeded) return Forbid();

        return Ok(order);
    }
}
```

### 6. Security Misconfiguration

```csharp
// ✅ GOOD: Secure headers middleware
app.UseSecurityHeaders(policies =>
    policies
        .AddDefaultSecurityHeaders()
        .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 31536000)
        .AddContentSecurityPolicy(builder =>
        {
            builder.AddDefaultSrc().Self();
            builder.AddScriptSrc().Self();
            builder.AddStyleSrc().Self().UnsafeInline();
        })
        .RemoveServerHeader());

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .AllowCredentials()
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Authorization", "Content-Type");
    });
});

// ✅ GOOD: Disable detailed errors in production
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}
```

### 7. Cross-Site Scripting (XSS)

```csharp
// ✅ GOOD: Use automatic encoding (Razor does this by default)
<p>@Model.UserInput</p>

// ✅ GOOD: HTML encode in API responses
public class UserDto
{
    public string Name { get; init; } = string.Empty;
    public string SafeName => WebUtility.HtmlEncode(Name);
}

// ✅ GOOD: Content-Type headers
return Ok(data); // ASP.NET Core sets Content-Type correctly

// ❌ BAD: Building HTML strings
var html = $"<div>{userInput}</div>"; // XSS vulnerability
```

### 8. Insecure Deserialization

```csharp
// ✅ GOOD: Use System.Text.Json with options
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

// ✅ GOOD: Explicit type mapping
var user = JsonSerializer.Deserialize<UserDto>(json, options);

// ❌ BAD: Deserializing to object
var obj = JsonSerializer.Deserialize<object>(json); // Type confusion risk
```

### 9. Using Components with Known Vulnerabilities

```bash
# Check for vulnerable packages
dotnet list package --vulnerable

# Update packages
dotnet outdated
dotnet add package PackageName --version X.Y.Z

# In CI/CD pipeline
- name: Check for vulnerabilities
  run: dotnet list package --vulnerable --include-transitive
```

### 10. Insufficient Logging & Monitoring

```csharp
// ✅ GOOD: Structured security logging
public class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        logger.LogInformation(
            "Request {Method} {Path} by user {UserId} from {IpAddress}",
            context.Request.Method,
            context.Request.Path,
            userId ?? "anonymous",
            context.Connection.RemoteIpAddress);

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Request {Method} {Path} failed for user {UserId}",
                context.Request.Method,
                context.Request.Path,
                userId ?? "anonymous");
            throw;
        }
    }
}

// Security event logging
public void LogSecurityEvent(string eventType, string details, string? userId = null)
{
    _logger.LogWarning(
        "SECURITY_EVENT: {EventType} | User: {UserId} | Details: {Details}",
        eventType,
        userId ?? "anonymous",
        details);
}
```

## Input Validation

```csharp
// FluentValidation example
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .Matches("[A-Z]").WithMessage("Must contain uppercase")
            .Matches("[a-z]").WithMessage("Must contain lowercase")
            .Matches("[0-9]").WithMessage("Must contain digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain special character");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z\\s-']+$").WithMessage("Invalid characters");
    }
}
```

## Secrets Management

```yaml
# appsettings.json (no secrets)
{
  "ConnectionStrings": {
    "DefaultConnection": "" # Set via environment
  },
  "Jwt": {
    "Issuer": "https://api.example.com",
    "Audience": "https://app.example.com"
    # Key set via environment
  }
}
```

```bash
# .env.example (template only)
DB_CONNECTION_STRING=
JWT_KEY=
SENDGRID_API_KEY=

# Never commit .env files
# .gitignore
.env
.env.*
!.env.example
```

## Security Checklist

### Per Story
- [ ] Input validation implemented
- [ ] Authorization checks in place
- [ ] Sensitive data not logged
- [ ] Error messages don't leak info
- [ ] SQL queries parameterized

### Per Release
- [ ] Dependency vulnerability scan
- [ ] SAST scan completed
- [ ] Secrets audit (no hardcoded secrets)
- [ ] Security headers configured
- [ ] CORS policy reviewed

### Infrastructure
- [ ] HTTPS enforced
- [ ] Security headers enabled
- [ ] Rate limiting configured
- [ ] Logging and monitoring active
- [ ] Backup and recovery tested
