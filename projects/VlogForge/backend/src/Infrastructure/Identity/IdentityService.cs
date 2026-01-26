using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Identity;

/// <summary>
/// Implementation of identity services using ASP.NET Identity password hasher.
/// Story: ACF-001
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        // PasswordHasher uses a dummy user, only the password is hashed
        return _passwordHasher.HashPassword(null!, password);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, string hash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hash, password);
        return result != PasswordVerificationResult.Failed;
    }

    /// <inheritdoc />
    public PasswordValidationResult ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return PasswordValidationResult.Failure(errors);
        }

        if (password.Length < 8)
            errors.Add("Password must be at least 8 characters.");

        if (password.Length > 128)
            errors.Add("Password must not exceed 128 characters.");

        if (!password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit.");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("Password must contain at least one special character.");

        return errors.Count > 0
            ? PasswordValidationResult.Failure(errors)
            : PasswordValidationResult.Success();
    }

    /// <inheritdoc />
    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
