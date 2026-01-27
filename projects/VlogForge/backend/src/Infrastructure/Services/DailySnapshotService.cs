using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Analytics.Commands.CreateSnapshot;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// Background service that creates daily metric snapshots at midnight UTC.
/// Story: ACF-004
/// </summary>
public sealed partial class DailySnapshotService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailySnapshotService> _logger;

    public DailySnapshotService(
        IServiceProvider serviceProvider,
        ILogger<DailySnapshotService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogServiceStarting(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            LogNextRunScheduled(_logger, nextMidnight, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);

                // Create snapshots for the previous day
                var snapshotDate = DateTime.UtcNow.Date.AddDays(-1);
                await CreateSnapshotsAsync(snapshotDate, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (Exception ex)
            {
                LogSnapshotError(_logger, ex);

                // Wait a bit before retrying to avoid tight loop on persistent errors
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        LogServiceStopping(_logger);
    }

    private async Task CreateSnapshotsAsync(DateTime snapshotDate, CancellationToken cancellationToken)
    {
        LogCreatingSnapshots(_logger, snapshotDate);

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new CreateDailySnapshotCommand(snapshotDate),
            cancellationToken);

        LogSnapshotsCompleted(_logger, result.SnapshotsCreated, result.UsersProcessed, result.Errors.Count);

        if (result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                LogSnapshotWarning(_logger, error);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Daily snapshot service starting")]
    private static partial void LogServiceStarting(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Daily snapshot service stopping")]
    private static partial void LogServiceStopping(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Next snapshot scheduled at {NextRun} (in {Delay})")]
    private static partial void LogNextRunScheduled(ILogger logger, DateTime nextRun, TimeSpan delay);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error creating daily snapshots")]
    private static partial void LogSnapshotError(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating daily snapshots for {Date}")]
    private static partial void LogCreatingSnapshots(ILogger logger, DateTime date);

    [LoggerMessage(Level = LogLevel.Information, Message = "Daily snapshot completed. Created: {Snapshots}, Users: {Users}, Errors: {Errors}")]
    private static partial void LogSnapshotsCompleted(ILogger logger, int snapshots, int users, int errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Snapshot error: {Error}")]
    private static partial void LogSnapshotWarning(ILogger logger, string error);
}
