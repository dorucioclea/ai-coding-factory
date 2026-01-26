using VlogForge.Domain.Common;
using VlogForge.Domain.Events;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Domain.Entities;

/// <summary>
/// User aggregate root representing a registered user in the system.
/// </summary>
public sealed class User : AggregateRoot
{
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
    /// Gets the email verification token.
    /// </summary>
    public string? EmailVerificationToken { get; private set; }

    /// <summary>
    /// Gets the email verification token expiry.
    /// </summary>
    public DateTime? EmailVerificationTokenExpiry { get; private set; }

    /// <summary>
    /// Gets the password reset token.
    /// </summary>
    public string? PasswordResetToken { get; private set; }

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
    /// </summary>
    public static User Create(Email email, string displayName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        if (displayName.Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters.", nameof(displayName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        var user = new User(email, displayName.Trim(), passwordHash);
        user.GenerateEmailVerificationToken();
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value, user.DisplayName));

        return user;
    }

    /// <summary>
    /// Generates a new email verification token.
    /// </summary>
    public void GenerateEmailVerificationToken()
    {
        EmailVerificationToken = Guid.NewGuid().ToString("N");
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        IncrementVersion();
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

        if (EmailVerificationToken != token)
            return false;

        if (EmailVerificationTokenExpiry < DateTime.UtcNow)
            return false;

        EmailVerified = true;
        EmailVerificationToken = null;
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
    /// </summary>
    public string GeneratePasswordResetToken()
    {
        PasswordResetToken = Guid.NewGuid().ToString("N");
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        IncrementVersion();

        RaiseDomainEvent(new PasswordResetRequestedEvent(Id, Email.Value));
        return PasswordResetToken;
    }

    /// <summary>
    /// Resets the password using the provided token.
    /// </summary>
    public bool ResetPassword(string token, string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        if (PasswordResetToken != token)
            return false;

        if (PasswordResetTokenExpiry < DateTime.UtcNow)
            return false;

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            return false;

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        IncrementVersion();

        RaiseDomainEvent(new PasswordResetCompletedEvent(Id, Email.Value));
        return true;
    }

    /// <summary>
    /// Adds a refresh token for this user.
    /// </summary>
    public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string? ipAddress = null, string? userAgent = null)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresAt, ipAddress, userAgent);
        _refreshTokens.Add(refreshToken);
        IncrementVersion();
        return refreshToken;
    }

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    public bool RevokeRefreshToken(string token, string? revokedByIp = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);
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
    /// Gets an active refresh token by its value.
    /// </summary>
    public RefreshToken? GetActiveRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);
    }

    /// <summary>
    /// Updates the user's display name.
    /// </summary>
    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        if (displayName.Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters.", nameof(displayName));

        DisplayName = displayName.Trim();
        IncrementVersion();
    }
}
