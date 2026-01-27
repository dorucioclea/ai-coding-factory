using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service interface for generating and validating OAuth state parameters.
/// Uses HMAC signing for integrity and includes expiration.
/// Story: ACF-003
/// </summary>
public interface IOAuthStateService
{
    /// <summary>
    /// Generates a signed OAuth state parameter.
    /// </summary>
    /// <param name="userId">The user initiating the OAuth flow.</param>
    /// <param name="platformType">The platform being connected.</param>
    /// <returns>A signed, base64-encoded state string.</returns>
    string GenerateState(Guid userId, PlatformType platformType);

    /// <summary>
    /// Validates and parses an OAuth state parameter.
    /// </summary>
    /// <param name="state">The state parameter from OAuth callback.</param>
    /// <returns>The parsed state data if valid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if state is invalid, tampered, or expired.</exception>
    OAuthStateData ValidateState(string state);
}

/// <summary>
/// Parsed OAuth state data.
/// </summary>
public record OAuthStateData(
    Guid UserId,
    PlatformType PlatformType,
    DateTime CreatedAt);
