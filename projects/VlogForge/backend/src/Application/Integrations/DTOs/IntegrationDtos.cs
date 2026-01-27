using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.DTOs;

/// <summary>
/// DTO for platform connection status.
/// Story: ACF-003
/// </summary>
public record PlatformConnectionDto(
    Guid Id,
    string PlatformType,
    string Status,
    string PlatformAccountId,
    string PlatformAccountName,
    DateTime? LastSyncAt,
    string? ErrorMessage,
    DateTime CreatedAt);

/// <summary>
/// DTO for all connection statuses response.
/// Story: ACF-003
/// </summary>
public record ConnectionStatusResponse(
    IReadOnlyList<PlatformConnectionDto> Connections,
    IReadOnlyList<string> AvailablePlatforms);

/// <summary>
/// DTO for OAuth initiation response.
/// Story: ACF-003
/// </summary>
public record OAuthInitiationResponse(
    string AuthorizationUrl,
    string State);

/// <summary>
/// DTO for OAuth completion request.
/// Story: ACF-003
/// </summary>
public record OAuthCompletionRequest(
    string Code,
    string State);

/// <summary>
/// DTO for OAuth completion response.
/// Story: ACF-003
/// </summary>
public record OAuthCompletionResponse(
    Guid ConnectionId,
    string PlatformType,
    string PlatformAccountId,
    string PlatformAccountName,
    string Status);

/// <summary>
/// DTO for disconnect response.
/// Story: ACF-003
/// </summary>
public record DisconnectResponse(
    string PlatformType,
    bool Success,
    string Message);

/// <summary>
/// Mapping extensions for integration DTOs.
/// Story: ACF-003
/// </summary>
public static class IntegrationMappingExtensions
{
    public static PlatformConnectionDto ToDto(this PlatformConnection connection)
    {
        return new PlatformConnectionDto(
            connection.Id,
            connection.PlatformType.ToString(),
            connection.Status.ToString(),
            connection.PlatformAccountId,
            connection.PlatformAccountName,
            connection.LastSyncAt,
            connection.ErrorMessage,
            connection.CreatedAt);
    }
}
