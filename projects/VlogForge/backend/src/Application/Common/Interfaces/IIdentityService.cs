namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service for identity-related operations like password hashing.
/// Story: ACF-001
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Hashes a password using a secure algorithm.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The password hash.</param>
    /// <returns>True if the password matches the hash.</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Validates password strength against security requirements.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>Validation result with any errors.</returns>
    PasswordValidationResult ValidatePassword(string password);

    /// <summary>
    /// Hashes a token (for refresh tokens, verification tokens, etc.) using SHA-256.
    /// </summary>
    /// <param name="token">The plain text token.</param>
    /// <returns>The hashed token.</returns>
    string HashToken(string token);
}

/// <summary>
/// Result of password validation.
/// </summary>
public sealed class PasswordValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    private PasswordValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static PasswordValidationResult Success() => new(true, Array.Empty<string>());
    public static PasswordValidationResult Failure(IEnumerable<string> errors) => new(false, errors.ToList().AsReadOnly());
}
