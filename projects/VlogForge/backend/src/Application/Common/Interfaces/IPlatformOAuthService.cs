using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service interface for platform-specific OAuth operations.
/// Story: ACF-003
/// </summary>
public interface IPlatformOAuthService
{
    /// <summary>
    /// Gets the supported platform type.
    /// </summary>
    PlatformType PlatformType { get; }

    /// <summary>
    /// Generates the OAuth authorization URL.
    /// </summary>
    /// <param name="state">State parameter for CSRF protection.</param>
    /// <param name="redirectUri">Callback URL after authorization.</param>
    /// <returns>The authorization URL to redirect the user to.</returns>
    string GetAuthorizationUrl(string state, string redirectUri);

    /// <summary>
    /// Exchanges the authorization code for access and refresh tokens.
    /// </summary>
    /// <param name="code">The authorization code from OAuth callback.</param>
    /// <param name="redirectUri">The redirect URI used in authorization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token response with access token, refresh token, and expiry.</returns>
    Task<OAuthTokenResponse> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token response with new access token and optionally new refresh token.</returns>
    Task<OAuthTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the access and refresh tokens.
    /// </summary>
    /// <param name="accessToken">The access token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeTokenAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's account information from the platform.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Platform account information.</returns>
    Task<PlatformAccountInfo> GetAccountInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// OAuth token response from platform API.
/// </summary>
public record OAuthTokenResponse(
    string AccessToken,
    string? RefreshToken,
    DateTime ExpiresAt,
    string? Scope);

/// <summary>
/// Platform account information.
/// </summary>
public record PlatformAccountInfo(
    string AccountId,
    string AccountName,
    string? ProfileUrl,
    int? FollowerCount);
