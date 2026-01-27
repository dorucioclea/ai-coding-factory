using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Analytics.Commands.CreateSnapshot;

/// <summary>
/// Handler for CreateDailySnapshotCommand.
/// Creates daily metric snapshots for historical trend analysis.
/// Story: ACF-004
/// </summary>
public sealed partial class CreateDailySnapshotCommandHandler
    : IRequestHandler<CreateDailySnapshotCommand, CreateSnapshotResult>
{
    private readonly IPlatformMetricsRepository _metricsRepository;
    private readonly IMetricsSnapshotRepository _snapshotRepository;
    private readonly IPlatformConnectionRepository _connectionRepository;
    private readonly ILogger<CreateDailySnapshotCommandHandler> _logger;

    public CreateDailySnapshotCommandHandler(
        IPlatformMetricsRepository metricsRepository,
        IMetricsSnapshotRepository snapshotRepository,
        IPlatformConnectionRepository connectionRepository,
        ILogger<CreateDailySnapshotCommandHandler> logger)
    {
        _metricsRepository = metricsRepository;
        _snapshotRepository = snapshotRepository;
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<CreateSnapshotResult> Handle(
        CreateDailySnapshotCommand request,
        CancellationToken cancellationToken)
    {
        var snapshotsCreated = 0;
        var usersProcessed = new HashSet<Guid>();
        var errors = new List<string>();

        // Get all active connections in a single query
        var connections = await _connectionRepository.GetAllActiveConnectionsAsync(cancellationToken);
        var connectionList = connections.ToList();

        if (connectionList.Count == 0)
        {
            return new CreateSnapshotResult(0, 0, errors);
        }

        // Batch load all metrics for active connections (single query)
        var connectionIds = connectionList.Select(c => c.Id);
        var metricsMap = await _metricsRepository.GetByConnectionIdsAsync(connectionIds, cancellationToken);

        // Batch load existing snapshots for this date (single query)
        var existingSnapshots = await _snapshotRepository.GetExistingForDateAsync(
            request.SnapshotDate, cancellationToken);

        var snapshotsToCreate = new List<MetricsSnapshot>();

        foreach (var connection in connectionList)
        {
            try
            {
                // Check if we have metrics (from pre-loaded map)
                if (!metricsMap.TryGetValue(connection.Id, out var metrics))
                {
                    LogNoMetrics(_logger, connection.Id);
                    continue;
                }

                // Check if snapshot already exists (from pre-loaded set)
                if (existingSnapshots.Contains((connection.UserId, connection.PlatformType)))
                {
                    LogSnapshotExists(_logger, connection.UserId, connection.PlatformType, request.SnapshotDate);
                    continue;
                }

                // Create snapshot from current metrics
                var snapshot = MetricsSnapshot.Create(
                    connection.UserId,
                    connection.PlatformType,
                    request.SnapshotDate,
                    metrics);

                snapshotsToCreate.Add(snapshot);
                usersProcessed.Add(connection.UserId);

                LogSnapshotCreated(_logger, connection.UserId, connection.PlatformType);
            }
            catch (Exception ex)
            {
                var error = $"Failed to create snapshot for connection {connection.Id}: {ex.Message}";
                errors.Add(error);
                LogSnapshotError(_logger, connection.Id, ex);
            }
        }

        // Batch add all snapshots
        if (snapshotsToCreate.Count > 0)
        {
            await _snapshotRepository.AddRangeAsync(snapshotsToCreate, cancellationToken);
            snapshotsCreated = snapshotsToCreate.Count;
        }

        // Save all snapshots
        await _snapshotRepository.SaveChangesAsync(cancellationToken);

        LogSnapshotComplete(_logger, snapshotsCreated, usersProcessed.Count, request.SnapshotDate);

        return new CreateSnapshotResult(snapshotsCreated, usersProcessed.Count, errors);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "No metrics found for connection {ConnectionId}")]
    private static partial void LogNoMetrics(ILogger logger, Guid connectionId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Snapshot already exists for user {UserId}, platform {Platform}, date {Date}")]
    private static partial void LogSnapshotExists(ILogger logger, Guid userId, PlatformType platform, DateTime date);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Created snapshot for user {UserId}, platform {Platform}")]
    private static partial void LogSnapshotCreated(ILogger logger, Guid userId, PlatformType platform);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create snapshot for connection {ConnectionId}")]
    private static partial void LogSnapshotError(ILogger logger, Guid connectionId, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created {SnapshotsCreated} snapshots for {UsersProcessed} users on {Date}")]
    private static partial void LogSnapshotComplete(ILogger logger, int snapshotsCreated, int usersProcessed, DateTime date);
}
