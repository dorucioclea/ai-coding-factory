# /add-auth - Add JWT Authentication

Add JWT authentication and authorization to ASP.NET Core applications.

## Usage
```
/add-auth [options]
```

Options:
- `--refresh-tokens` - Include refresh token support
- `--roles` - Include role-based authorization
- `--claims` - Include claims-based authorization
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Analyze Project Structure

Verify:
- Clean Architecture layers exist
- Infrastructure project for security implementation
- Application project for auth commands/queries
- API project for controllers and middleware

### 2. Create Authentication Structure

```
src/{{ProjectName}}.Infrastructure/Security/
├── Jwt/
│   ├── IJwtTokenGenerator.cs
│   ├── JwtTokenGenerator.cs
│   └── JwtSettings.cs
├── Password/
│   ├── IPasswordHasher.cs
│   └── PasswordHasher.cs
└── Extensions/
    └── AuthenticationExtensions.cs

src/{{ProjectName}}.Application/Authentication/
├── Commands/
│   ├── Register/
│   │   ├── RegisterCommand.cs
│   │   ├── RegisterCommandHandler.cs
│   │   └── RegisterCommandValidator.cs
│   ├── Login/
│   │   ├── LoginCommand.cs
│   │   ├── LoginCommandHandler.cs
│   │   └── LoginCommandValidator.cs
│   └── RefreshToken/
│       ├── RefreshTokenCommand.cs
│       └── RefreshTokenCommandHandler.cs
└── DTOs/
    ├── AuthResponse.cs
    ├── LoginRequest.cs
    └── RegisterRequest.cs

src/{{ProjectName}}.Api/Controllers/
└── AuthController.cs
```

### 3. Generate JWT Token Generator

**IJwtTokenGenerator.cs**:
```csharp
namespace {{Namespace}}.Infrastructure.Security.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, IList<string> roles);
    string GenerateRefreshToken();
}
```

**JwtTokenGenerator.cs**:
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace {{Namespace}}.Infrastructure.Security.Jwt;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(Guid userId, string email, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        claims.AddRange(roles.Select(role =>
            new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
```

**JwtSettings.cs**:
```csharp
namespace {{Namespace}}.Infrastructure.Security.Jwt;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
```

### 4. Generate Password Hasher

**IPasswordHasher.cs**:
```csharp
namespace {{Namespace}}.Infrastructure.Security.Password;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
```

**PasswordHasher.cs**:
```csharp
using System.Security.Cryptography;

namespace {{Namespace}}.Infrastructure.Security.Password;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
```

### 5. Generate Authentication Extension

**AuthenticationExtensions.cs**:
```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using {{Namespace}}.Infrastructure.Security.Jwt;
using {{Namespace}}.Infrastructure.Security.Password;

namespace {{Namespace}}.Infrastructure.Security.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("UserOrAdmin", policy =>
                policy.RequireRole("User", "Admin"));
        });

        return services;
    }
}
```

### 6. Generate Login Command

**LoginCommand.cs**:
```csharp
using MediatR;
using {{Namespace}}.Application.Authentication.DTOs;
using {{Namespace}}.Domain.Common;

namespace {{Namespace}}.Application.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;
```

**LoginCommandHandler.cs**:
```csharp
using MediatR;
using {{Namespace}}.Application.Authentication.DTOs;
using {{Namespace}}.Domain.Common;
using {{Namespace}}.Domain.Repositories;
using {{Namespace}}.Infrastructure.Security.Jwt;
using {{Namespace}}.Infrastructure.Security.Password;

namespace {{Namespace}}.Application.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            return Result<AuthResponse>.Failure("Invalid credentials");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid credentials");

        var roles = await _userRepository.GetRolesAsync(user.Id, cancellationToken);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Store refresh token (implement based on your needs)
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60)));
    }
}
```

### 7. Generate Auth Controller

**AuthController.cs**:
```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using {{Namespace}}.Application.Authentication.Commands.Login;
using {{Namespace}}.Application.Authentication.Commands.Register;
using {{Namespace}}.Application.Authentication.Commands.RefreshToken;
using {{Namespace}}.Application.Authentication.DTOs;

namespace {{Namespace}}.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.Name);
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(Register), result.Value);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Refresh access token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        return Ok(result.Value);
    }
}
```

### 8. Update appsettings.json

Add JWT configuration:
```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key-minimum-32-characters",
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

## Security Checklist

- [ ] Secret key is at least 256 bits (32 characters)
- [ ] Secret stored in environment variable or secrets manager
- [ ] Password hashing uses modern algorithm (PBKDF2/Argon2)
- [ ] Token expiry is reasonable (1 hour access, 7 days refresh)
- [ ] Refresh token rotation implemented
- [ ] HTTPS required in production
- [ ] Rate limiting on auth endpoints

## Output

```markdown
## JWT Authentication Added

### Files Created
**Infrastructure Layer:**
- Security/Jwt/IJwtTokenGenerator.cs
- Security/Jwt/JwtTokenGenerator.cs
- Security/Jwt/JwtSettings.cs
- Security/Password/IPasswordHasher.cs
- Security/Password/PasswordHasher.cs
- Security/Extensions/AuthenticationExtensions.cs

**Application Layer:**
- Authentication/Commands/Login/LoginCommand.cs
- Authentication/Commands/Login/LoginCommandHandler.cs
- Authentication/Commands/Register/RegisterCommand.cs
- Authentication/Commands/Register/RegisterCommandHandler.cs
- Authentication/DTOs/AuthResponse.cs
- Authentication/DTOs/LoginRequest.cs
- Authentication/DTOs/RegisterRequest.cs

**API Layer:**
- Controllers/AuthController.cs

### Configuration Required
Add to appsettings.json (use secrets in production):
```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret",
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com"
  }
}
```

### Registration
Add to Program.cs:
```csharp
builder.Services.AddJwtAuthentication(builder.Configuration);
```

### Usage
- POST /api/auth/register - Register new user
- POST /api/auth/login - Get access token
- POST /api/auth/refresh - Refresh access token
```

## Example

```
User: /add-auth --refresh-tokens --roles --story ACF-060

Claude: Adding JWT authentication with refresh tokens and role-based authorization...

Creating security infrastructure:
1. JWT token generator with refresh token support
2. Password hasher (PBKDF2-SHA256)
3. Authentication extension for DI

Creating application layer:
4. Register command with validation
5. Login command with refresh token
6. RefreshToken command

Creating API layer:
7. AuthController with endpoints

JWT authentication added successfully.

Next steps:
1. Add JwtSettings to appsettings.json
2. Call builder.Services.AddJwtAuthentication() in Program.cs
3. Add User entity if not exists
4. Implement IUserRepository
```
