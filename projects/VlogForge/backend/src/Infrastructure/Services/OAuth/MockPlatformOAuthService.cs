using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Services.OAuth;

/// <summary>
/// Mock OAuth service for development and testing.
/// Story: ACF-003
/// </summary>
public class MockYouTubeOAuthService : IPlatformOAuthService
{
    public PlatformType PlatformType => PlatformType.YouTube;

    public string GetAuthorizationUrl(string state, string redirectUri)
    {
        // Return a mock authorization URL
        return $"https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id=mock&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope=https://www.googleapis.com/auth/youtube.readonly&state={Uri.EscapeDataString(state)}&access_type=offline&prompt=consent";
    }

    public Task<OAuthTokenResponse> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        // Return mock tokens
        return Task.FromResult(new OAuthTokenResponse(
            AccessToken: $"mock_yt_access_{Guid.NewGuid():N}",
            RefreshToken: $"mock_yt_refresh_{Guid.NewGuid():N}",
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            Scope: "https://www.googleapis.com/auth/youtube.readonly"));
    }

    public Task<OAuthTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OAuthTokenResponse(
            AccessToken: $"mock_yt_access_{Guid.NewGuid():N}",
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            Scope: null));
    }

    public Task RevokeTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<PlatformAccountInfo> GetAccountInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PlatformAccountInfo(
            AccountId: "UC" + Guid.NewGuid().ToString("N")[..16],
            AccountName: "Mock YouTube Channel",
            ProfileUrl: "https://youtube.com/@mockchannel",
            FollowerCount: 10000));
    }
}

/// <summary>
/// Mock Instagram OAuth service for development and testing.
/// Story: ACF-003
/// </summary>
public class MockInstagramOAuthService : IPlatformOAuthService
{
    public PlatformType PlatformType => PlatformType.Instagram;

    public string GetAuthorizationUrl(string state, string redirectUri)
    {
        return $"https://api.instagram.com/oauth/authorize?client_id=mock&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope=user_profile,user_media&response_type=code&state={Uri.EscapeDataString(state)}";
    }

    public Task<OAuthTokenResponse> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OAuthTokenResponse(
            AccessToken: $"mock_ig_access_{Guid.NewGuid():N}",
            RefreshToken: $"mock_ig_refresh_{Guid.NewGuid():N}",
            ExpiresAt: DateTime.UtcNow.AddDays(60),
            Scope: "user_profile,user_media"));
    }

    public Task<OAuthTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OAuthTokenResponse(
            AccessToken: $"mock_ig_access_{Guid.NewGuid():N}",
            RefreshToken: null,
            ExpiresAt: DateTime.UtcNow.AddDays(60),
            Scope: null));
    }

    public Task RevokeTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<PlatformAccountInfo> GetAccountInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PlatformAccountInfo(
            AccountId: Guid.NewGuid().ToString("N")[..16],
            AccountName: "mock_instagram_user",
            ProfileUrl: "https://instagram.com/mock_user",
            FollowerCount: 5000));
    }
}

/// <summary>
/// Mock TikTok OAuth service for development and testing.
/// Story: ACF-003
/// </summary>
public class MockTikTokOAuthService : IPlatformOAuthService
{
    public PlatformType PlatformType => PlatformType.TikTok;

    public string GetAuthorizationUrl(string state, string redirectUri)
    {
        return $"https://www.tiktok.com/auth/authorize?client_key=mock&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope=user.info.basic,video.list&response_type=code&state={Uri.EscapeDataString(state)}";
    }

    public Task<OAuthTokenResponse> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OAuthTokenResponse(
            AccessToken: $"mock_tt_access_{Guid.NewGuid():N}",
            RefreshToken: $"mock_tt_refresh_{Guid.NewGuid():N}",
            ExpiresAt: DateTime.UtcNow.AddDays(1),
            Scope: "user.info.basic,video.list"));
    }

    public Task<OAuthTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OAuthTokenResponse(
            AccessToken: $"mock_tt_access_{Guid.NewGuid():N}",
            RefreshToken: $"mock_tt_refresh_{Guid.NewGuid():N}",
            ExpiresAt: DateTime.UtcNow.AddDays(1),
            Scope: null));
    }

    public Task RevokeTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<PlatformAccountInfo> GetAccountInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PlatformAccountInfo(
            AccountId: Guid.NewGuid().ToString("N")[..16],
            AccountName: "@mock_tiktok_user",
            ProfileUrl: "https://tiktok.com/@mock_user",
            FollowerCount: 25000));
    }
}
