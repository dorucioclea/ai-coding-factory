using System.Security.Cryptography;
using VlogForge.Domain.Common;
using VlogForge.Domain.Events;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Domain.Entities;

/// <summary>
/// User aggregate root representing a registered user in the system.
/// </summary>
public sealed class User : AggregateRoot
{
    private const int MaxRefreshTokens = 10;
    private const int MinDisplayNameLength = 2;
    private const int MaxDisplayNameLength = 100;
    private const int PasswordResetTokenExpiryHours = 1;
    private const int EmailVerificationTokenExpiryHours = 24;

    private readonly List<RefreshToken> _refreshTokens = new();

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the hashed password.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Gets whether the email has been verified.
    /// </summary>
    public bool EmailVerified { get; private set; }

    /// <summary>
    /// Gets the email verification token (hashed).
    /// </summary>
    public string? EmailVerificationTokenHash { get; private set; }

    /// <summary>
    /// Gets the email verification token expiry.
    /// </summary>
    public DateTime? EmailVerificationTokenExpiry { get; private set; }

    /// <summary>
    /// Gets the password reset token (hashed).
    /// </summary>
    public string? PasswordResetTokenHash { get; private set; }

    /// <summary>
    /// Gets the password reset token expiry.
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    /// <summary>
    /// Gets the number of failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// Gets the lockout end time if the account is locked.
    /// </summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>
    /// Gets the last login timestamp.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Gets the refresh tokens associated with this user.
    /// </summary>
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() : base()
    {
        Email = null!;
        DisplayName = string.Empty;
        PasswordHash = string.Empty;
    }

    private User(Email email, string displayName, string passwordHash) : base()
    {
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        EmailVerified = false;
        FailedLoginAttempts = 0;
    }

    /// <summary>
    /// Creates a new user with the specified details.
    /// Returns the user and the plain text email verification token (to send via email).
    /// </summary>
    public static (User User, string VerificationToken) Create(Email email, string displayName, string passwordHash)
    {
        ValidateDisplayName(displayName);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        var user = new User(email, displayName.Trim(), passwordHash);
        var plainToken = user.GenerateEmailVerificationToken();
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value, user.DisplayName));

        return (user, plainToken);
    }

    /// <summary>
    /// Generates a new email verification token.
    /// Returns the plain text token (to send via email).
    /// </summary>
    public string GenerateEmailVerificationToken()
    {
        var plainToken = GenerateSecureToken();
        EmailVerificationTokenHash = HashToken(plainToken);
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(EmailVerificationTokenExpiryHours);
        IncrementVersion();
        return plainToken;
    }

    /// <summary>
    /// Verifies the user's email with the provided token.
    /// </summary>
    public bool VerifyEmail(string token)
    {
        if (EmailVerified)
            return true;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        if (EmailVerificationTokenHash == null)
            return false;

        if (!ConstantTimeEquals(HashToken(token), EmailVerificationTokenHash))
            return false;

        if (EmailVerificationTokenExpiry < DateTime.UtcNow)
            return false;

        EmailVerified = true;
        EmailVerificationTokenHash = null;
        EmailVerificationTokenExpiry = null;
        IncrementVersion();

        RaiseDomainEvent(new UserEmailVerifiedEvent(Id, Email.Value));
        return true;
    }

    /// <summary>
    /// Records a successful login.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginAt = DateTime.UtcNow;
        IncrementVersion();

        RaiseDomainEvent(new UserLoggedInEvent(Id, Email.Value));
    }

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            RaiseDomainEvent(new UserLockedOutEvent(Id, Email.Value, LockoutEnd.Value));
        }

        IncrementVersion();
    }

    /// <summary>
    /// Checks if the account is currently locked out.
    /// </summary>
    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a password reset token.
    /// Returns the plain text token (to send via email).
    /// </summary>
    public string GeneratePasswordResetToken()
    {
        var plainToken = GenerateSecureToken();
        PasswordResetTokenHash = HashToken(plainToken);
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(PasswordResetTokenExpiryHours);
        IncrementVersion();

        RaiseDomainEvent(new PasswordResetRequestedEvent(Id, Email.Value));
        return plainToken;
    }

    /// <summary>
    /// Resets the password using the provided token.
    /// </summary>
    public bool ResetPassword(string token, string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        if (PasswordResetTokenHash == null)
            return false;

        if (!ConstantTimeEquals(HashToken(token), PasswordResetTokenHash))
            return false;

        if (PasswordResetTokenExpiry < DateTime.UtcNow)
            return false;

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            return false;

        PasswordHash = newPasswordHash;
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiry = null;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        IncrementVersion();

        RaiseDomainEvent(new PasswordResetCompletedEvent(Id, Email.Value));
        return true;
    }

    /// <summary>
    /// Adds a refresh token for this user.
    /// Returns the created RefreshToken entity.
    /// </summary>
    public RefreshToken AddRefreshToken(string tokenHash, DateTime expiresAt, string? ipAddress = null, string? userAgent = null)
    {
        // Clean up expired tokens
        _refreshTokens.RemoveAll(t => t.IsExpired);

        // Revoke oldest active tokens if at limit
        var activeTokens = _refreshTokens.Where(t => t.IsActive).OrderBy(t => t.CreatedAt).ToList();
        while (activeTokens.Count >= MaxRefreshTokens)
        {
            var oldest = activeTokens.First();
            oldest.Revoke("System: Token limit reached");
            activeTokens.Remove(oldest);
        }

        var refreshToken = RefreshToken.Create(Id, tokenHash, expiresAt, ipAddress, userAgent);
        _refreshTokens.Add(refreshToken);
        IncrementVersion();
        return refreshToken;
    }

    /// <summary>
    /// Revokes a specific refresh token by its hash.
    /// </summary>
    public bool RevokeRefreshToken(string tokenHash, string? revokedByIp = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => ConstantTimeEquals(t.TokenHash, tokenHash) && t.IsActive);
        if (refreshToken == null)
            return false;

        refreshToken.Revoke(revokedByIp);
        IncrementVersion();
        return true;
    }

    /// <summary>
    /// Revokes all refresh tokens for this user.
    /// </summary>
    public void RevokeAllRefreshTokens(string? revokedByIp = null)
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(revokedByIp);
        }
        IncrementVersion();
    }

    /// <summary>
    /// Gets an active refresh token by its hash.
    /// </summary>
    public RefreshToken? GetActiveRefreshToken(string tokenHash)
    {
        return _refreshTokens.FirstOrDefault(t => ConstantTimeEquals(t.TokenHash, tokenHash) && t.IsActive);
    }

    /// <summary>
    /// Updates the user's display name.
    /// </summary>
    public void UpdateDisplayName(string displayName)
    {
        ValidateDisplayName(displayName);
        DisplayName = displayName.Trim();
        IncrementVersion();
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    private static string GenerateSecureToken(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Hashes a token using SHA-256.
    /// </summary>
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Performs constant-time string comparison to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a == null || b == null)
            return a == b;

        var aBytes = System.Text.Encoding.UTF8.GetBytes(a);
        var bBytes = System.Text.Encoding.UTF8.GetBytes(b);

        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    /// <summary>
    /// Validates display name according to business rules.
    /// </summary>
    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        var trimmed = displayName.Trim();

        if (trimmed.Length < MinDisplayNameLength)
            throw new ArgumentException($"Display name must be at least {MinDisplayNameLength} characters.", nameof(displayName));

        if (trimmed.Length > MaxDisplayNameLength)
            throw new ArgumentException($"Display name cannot exceed {MaxDisplayNameLength} characters.", nameof(displayName));
    }
}
