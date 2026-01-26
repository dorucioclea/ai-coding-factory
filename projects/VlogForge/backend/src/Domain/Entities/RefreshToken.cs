using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Entity representing a refresh token for JWT authentication.
/// Stores the token hash, not the plain token, for security.
/// </summary>
public sealed class RefreshToken : Entity
{
    private const int MaxUserAgentLength = 500;

    /// <summary>
    /// Gets the user ID this token belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the hashed token value (SHA-256).
    /// The plain token should never be stored.
    /// </summary>
    public string TokenHash { get; private set; }

    /// <summary>
    /// Gets the token expiration date.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the date when this token was revoked, if applicable.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Gets the IP address or reason that revoked this token.
    /// </summary>
    public string? RevokedBy { get; private set; }

    /// <summary>
    /// Gets the token hash that replaced this one.
    /// </summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>
    /// Gets the IP address that created this token.
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// Gets the user agent that created this token (truncated for safety).
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
        TokenHash = string.Empty;
    }

    private RefreshToken(Guid userId, string tokenHash, DateTime expiresAt, string? createdByIp, string? userAgent) : base()
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedByIp = ValidateIpAddress(createdByIp);
        UserAgent = TruncateUserAgent(userAgent);
    }

    /// <summary>
    /// Creates a new refresh token with the hashed token value.
    /// The caller is responsible for hashing the token before calling this method.
    /// </summary>
    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt, string? createdByIp = null, string? userAgent = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future.", nameof(expiresAt));

        return new RefreshToken(userId, tokenHash, expiresAt, createdByIp, userAgent);
    }

    /// <summary>
    /// Revokes this token.
    /// </summary>
    public void Revoke(string? revokedBy = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    /// <summary>
    /// Validates and normalizes an IP address.
    /// Returns null if the IP is invalid.
    /// </summary>
    private static string? ValidateIpAddress(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return null;

        if (System.Net.IPAddress.TryParse(ip.Trim(), out var parsed))
            return parsed.ToString();

        return null;
    }

    /// <summary>
    /// Truncates user agent to prevent storage of excessively long strings.
    /// </summary>
    private static string? TruncateUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return null;

        return userAgent.Length > MaxUserAgentLength
            ? userAgent[..MaxUserAgentLength]
            : userAgent;
    }
}
