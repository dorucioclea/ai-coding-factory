using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Analytics.Commands.SyncMetrics;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// Background service that periodically syncs analytics metrics from connected platforms.
/// Runs every 6 hours.
/// Story: ACF-004
/// </summary>
public sealed partial class AnalyticsSyncService : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalyticsSyncService> _logger;

    public AnalyticsSyncService(
        IServiceProvider serviceProvider,
        ILogger<AnalyticsSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogServiceStarting(_logger);

        // Wait a bit before first run to let the application fully start
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAllUsersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogSyncError(_logger, ex);
            }

            await Task.Delay(SyncInterval, stoppingToken);
        }

        LogServiceStopping(_logger);
    }

    private async Task SyncAllUsersAsync(CancellationToken cancellationToken)
    {
        LogSyncStarting(_logger);

        using var scope = _serviceProvider.CreateScope();
        var connectionRepository = scope.ServiceProvider
            .GetRequiredService<IPlatformConnectionRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Get all unique user IDs with active connections
        var connections = await connectionRepository
            .GetAllActiveConnectionsAsync(cancellationToken);

        var userIds = connections
            .Select(c => c.UserId)
            .Distinct()
            .ToList();

        LogUsersFound(_logger, userIds.Count);

        var totalPlatformsSynced = 0;
        var totalContentSynced = 0;
        var totalErrors = 0;

        foreach (var userId in userIds)
        {
            try
            {
                var result = await mediator.Send(
                    new SyncPlatformMetricsCommand(userId),
                    cancellationToken);

                totalPlatformsSynced += result.PlatformsSynced;
                totalContentSynced += result.ContentItemsSynced;
                totalErrors += result.Errors.Count;
            }
            catch (Exception ex)
            {
                totalErrors++;
                LogUserSyncError(_logger, userId, ex);
            }
        }

        LogSyncCompleted(_logger, totalPlatformsSynced, totalContentSynced, totalErrors);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Analytics sync service starting")]
    private static partial void LogServiceStarting(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Analytics sync service stopping")]
    private static partial void LogServiceStopping(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error during analytics sync")]
    private static partial void LogSyncError(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting analytics sync for all users")]
    private static partial void LogSyncStarting(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Found {UserCount} users with active connections to sync")]
    private static partial void LogUsersFound(ILogger logger, int userCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to sync user {UserId}")]
    private static partial void LogUserSyncError(ILogger logger, Guid userId, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Analytics sync completed. Platforms: {Platforms}, Content: {Content}, Errors: {Errors}")]
    private static partial void LogSyncCompleted(ILogger logger, int platforms, int content, int errors);
}
