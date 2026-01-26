using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service for JWT token generation and validation.
/// Story: ACF-001
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>The JWT access token and its expiration.</returns>
    TokenResult GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>The plain text refresh token and its expiration.</returns>
    RefreshTokenResult GenerateRefreshToken();

    /// <summary>
    /// Validates an access token and extracts the user ID.
    /// </summary>
    /// <param name="token">The JWT access token.</param>
    /// <returns>The user ID if valid, null otherwise.</returns>
    Guid? ValidateAccessToken(string token);
}

/// <summary>
/// Result of access token generation.
/// </summary>
public sealed class TokenResult
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }

    public TokenResult(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }
}

/// <summary>
/// Result of refresh token generation.
/// </summary>
public sealed class RefreshTokenResult
{
    /// <summary>
    /// The plain text token (to send to client).
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// The hashed token (to store in database).
    /// </summary>
    public string TokenHash { get; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; }

    public RefreshTokenResult(string token, string tokenHash, DateTime expiresAt)
    {
        Token = token;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }
}
