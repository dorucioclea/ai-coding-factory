using MediatR;

namespace VlogForge.Application.Analytics.Commands.CreateSnapshot;

/// <summary>
/// Command to create daily snapshots for all users with connected platforms.
/// Story: ACF-004
/// </summary>
public sealed record CreateDailySnapshotCommand(
    DateTime SnapshotDate) : IRequest<CreateSnapshotResult>;

/// <summary>
/// Result of the snapshot creation.
/// </summary>
public record CreateSnapshotResult(
    int SnapshotsCreated,
    int UsersProcessed,
    IReadOnlyList<string> Errors);
