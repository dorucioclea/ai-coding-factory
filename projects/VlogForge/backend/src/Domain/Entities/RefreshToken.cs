using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Entity representing a refresh token for JWT authentication.
/// </summary>
public sealed class RefreshToken : Entity
{
    /// <summary>
    /// Gets the user ID this token belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the token value.
    /// </summary>
    public string Token { get; private set; }

    /// <summary>
    /// Gets the token expiration date.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the date when this token was revoked, if applicable.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Gets the IP address that revoked this token.
    /// </summary>
    public string? RevokedByIp { get; private set; }

    /// <summary>
    /// Gets the token that replaced this one.
    /// </summary>
    public string? ReplacedByToken { get; private set; }

    /// <summary>
    /// Gets the IP address that created this token.
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// Gets the user agent that created this token.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Gets whether this token is expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Gets whether this token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Gets whether this token is active (not expired and not revoked).
    /// </summary>
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken() : base()
    {
        Token = string.Empty;
    }

    private RefreshToken(Guid userId, string token, DateTime expiresAt, string? createdByIp, string? userAgent) : base()
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        UserAgent = userAgent;
    }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt, string? createdByIp = null, string? userAgent = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future.", nameof(expiresAt));

        return new RefreshToken(userId, token, expiresAt, createdByIp, userAgent);
    }

    /// <summary>
    /// Revokes this token.
    /// </summary>
    public void Revoke(string? revokedByIp = null, string? replacedByToken = null)
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
    }
}
