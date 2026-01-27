namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service interface for validating OAuth redirect URIs against a whitelist.
/// Story: ACF-003
/// </summary>
public interface IOAuthRedirectValidator
{
    /// <summary>
    /// Validates that a redirect URI is in the allowed whitelist.
    /// </summary>
    /// <param name="redirectUri">The redirect URI to validate.</param>
    /// <returns>True if the URI is allowed, false otherwise.</returns>
    bool IsAllowed(string redirectUri);

    /// <summary>
    /// Gets the list of allowed redirect URIs (for error messages).
    /// </summary>
    IReadOnlyList<string> AllowedUris { get; }
}
