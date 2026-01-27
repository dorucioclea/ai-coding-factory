using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations;

/// <summary>
/// Shared constants for platform integrations.
/// Story: ACF-003
/// </summary>
public static class IntegrationConstants
{
    /// <summary>
    /// Platforms supported for OAuth integration.
    /// </summary>
    public static readonly PlatformType[] SupportedPlatforms =
    {
        PlatformType.YouTube,
        PlatformType.Instagram,
        PlatformType.TikTok
    };

    /// <summary>
    /// Maximum length for authorization code from OAuth provider.
    /// </summary>
    public const int MaxAuthorizationCodeLength = 2000;

    /// <summary>
    /// Maximum length for OAuth state parameter.
    /// </summary>
    public const int MaxStateLength = 500;

    /// <summary>
    /// Maximum length for redirect URI.
    /// </summary>
    public const int MaxRedirectUriLength = 2000;

    /// <summary>
    /// OAuth state expiration time.
    /// </summary>
    public static readonly TimeSpan StateExpiration = TimeSpan.FromMinutes(10);
}
