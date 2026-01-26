namespace VlogForge.Application.Auth.DTOs;

/// <summary>
/// Response containing authentication tokens.
/// Story: ACF-001
/// </summary>
public sealed class AuthResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; init; }
    public bool EmailVerified { get; init; }
}

/// <summary>
/// Response for operations that don't return tokens.
/// Story: ACF-001
/// </summary>
public sealed class AuthResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static AuthResult Succeeded(string? message = null) => new() { Success = true, Message = message };
    public static AuthResult Failed(params string[] errors) => new() { Success = false, Errors = errors };
    public static AuthResult Failed(IEnumerable<string> errors) => new() { Success = false, Errors = errors.ToList().AsReadOnly() };
}

/// <summary>
/// User profile information.
/// Story: ACF-001
/// </summary>
public sealed class UserProfile
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
