namespace VlogForge.Api.Controllers.Integrations.Requests;

/// <summary>
/// Request to initiate OAuth connection.
/// Story: ACF-003
/// </summary>
public class InitiateOAuthRequest
{
    /// <summary>
    /// The redirect URI for OAuth callback.
    /// </summary>
    public required string RedirectUri { get; init; }
}

/// <summary>
/// Request to complete OAuth connection.
/// Story: ACF-003
/// </summary>
public class CompleteOAuthRequest
{
    /// <summary>
    /// The authorization code from OAuth provider.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The state parameter for CSRF validation.
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// The redirect URI used during authorization.
    /// </summary>
    public required string RedirectUri { get; init; }
}
