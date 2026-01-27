using MediatR;

namespace VlogForge.Application.Analytics.Commands.SyncMetrics;

/// <summary>
/// Command to sync metrics for a user's connected platforms.
/// Story: ACF-004
/// </summary>
public sealed record SyncPlatformMetricsCommand(
    Guid UserId,
    Guid? PlatformConnectionId = null) : IRequest<SyncMetricsResult>;

/// <summary>
/// Result of the sync operation.
/// </summary>
public record SyncMetricsResult(
    int PlatformsSynced,
    int ContentItemsSynced,
    IReadOnlyList<string> Errors);
