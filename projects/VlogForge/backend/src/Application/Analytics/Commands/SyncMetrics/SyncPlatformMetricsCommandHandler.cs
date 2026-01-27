using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Analytics.Commands.SyncMetrics;

/// <summary>
/// Handler for SyncPlatformMetricsCommand.
/// Syncs metrics from connected platforms using their APIs.
/// Story: ACF-004
/// </summary>
public sealed partial class SyncPlatformMetricsCommandHandler
    : IRequestHandler<SyncPlatformMetricsCommand, SyncMetricsResult>
{
    private readonly IPlatformConnectionRepository _connectionRepository;
    private readonly IPlatformMetricsRepository _metricsRepository;
    private readonly IContentPerformanceRepository _contentRepository;
    private readonly IEnumerable<IPlatformDataService> _platformServices;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<SyncPlatformMetricsCommandHandler> _logger;

    public SyncPlatformMetricsCommandHandler(
        IPlatformConnectionRepository connectionRepository,
        IPlatformMetricsRepository metricsRepository,
        IContentPerformanceRepository contentRepository,
        IEnumerable<IPlatformDataService> platformServices,
        IEncryptionService encryptionService,
        ILogger<SyncPlatformMetricsCommandHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _metricsRepository = metricsRepository;
        _contentRepository = contentRepository;
        _platformServices = platformServices;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<SyncMetricsResult> Handle(
        SyncPlatformMetricsCommand request,
        CancellationToken cancellationToken)
    {
        var platformsSynced = 0;
        var contentItemsSynced = 0;
        var errors = new List<string>();

        // Get connections to sync
        IEnumerable<PlatformConnection> connections;
        if (request.PlatformConnectionId.HasValue)
        {
            var connection = await _connectionRepository.GetByIdAsync(
                request.PlatformConnectionId.Value, cancellationToken);

            // Verify ownership - prevent unauthorized access to other users' connections
            if (connection != null && connection.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException(
                    "Cannot sync metrics for a connection you don't own.");
            }

            connections = connection != null ? [connection] : [];
        }
        else
        {
            var userConnections = await _connectionRepository.GetByUserIdAsync(
                request.UserId, cancellationToken);
            connections = userConnections.Where(c => c.Status == ConnectionStatus.Connected);
        }

        foreach (var connection in connections)
        {
            try
            {
                var service = _platformServices.FirstOrDefault(s =>
                    s.SupportedPlatform == connection.PlatformType);

                if (service == null)
                {
                    LogNoDataService(_logger, connection.PlatformType);
                    continue;
                }

                // Decrypt access token
                var accessToken = connection.EncryptedAccessToken != null
                    ? _encryptionService.Decrypt(connection.EncryptedAccessToken)
                    : string.Empty;

                if (string.IsNullOrEmpty(accessToken))
                {
                    errors.Add($"No access token for {connection.PlatformType}");
                    continue;
                }

                // Sync metrics
                var metricsData = await service.FetchMetricsAsync(accessToken, cancellationToken);

                await SyncPlatformMetricsAsync(connection, metricsData, cancellationToken);
                platformsSynced++;

                // Sync top content
                var contentData = await service.FetchTopContentAsync(accessToken, 20, cancellationToken);

                foreach (var content in contentData)
                {
                    await SyncContentAsync(connection, content, cancellationToken);
                    contentItemsSynced++;
                }

                LogMetricsSynced(_logger, connection.PlatformType, connection.Id);
            }
            catch (Exception ex)
            {
                var error = $"Failed to sync {connection.PlatformType}: {ex.Message}";
                errors.Add(error);
                LogSyncError(_logger, connection.PlatformType, ex);
            }
        }

        // Save all changes
        await _metricsRepository.SaveChangesAsync(cancellationToken);
        await _contentRepository.SaveChangesAsync(cancellationToken);

        return new SyncMetricsResult(platformsSynced, contentItemsSynced, errors);
    }

    private async Task SyncPlatformMetricsAsync(
        PlatformConnection connection,
        PlatformMetricsData data,
        CancellationToken cancellationToken)
    {
        var existing = await _metricsRepository.GetByConnectionIdAsync(
            connection.Id, cancellationToken);

        if (existing == null)
        {
            var metrics = PlatformMetrics.Create(
                connection.Id,
                connection.PlatformType,
                data.FollowerCount,
                data.TotalViews,
                data.TotalLikes,
                data.TotalComments,
                data.TotalShares);

            await _metricsRepository.AddAsync(metrics, cancellationToken);
        }
        else
        {
            existing.UpdateMetrics(
                data.FollowerCount,
                data.TotalViews,
                data.TotalLikes,
                data.TotalComments,
                data.TotalShares);

            await _metricsRepository.UpdateAsync(existing, cancellationToken);
        }
    }

    private async Task SyncContentAsync(
        PlatformConnection connection,
        ContentData data,
        CancellationToken cancellationToken)
    {
        var content = ContentPerformance.Create(
            connection.Id,
            connection.PlatformType,
            data.ContentId,
            data.Title,
            data.ThumbnailUrl,
            data.ContentUrl,
            data.PublishedAt,
            data.ViewCount,
            data.LikeCount,
            data.CommentCount,
            data.ShareCount);

        await _contentRepository.UpsertAsync(content, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No data service found for platform {Platform}")]
    private static partial void LogNoDataService(ILogger logger, PlatformType platform);

    [LoggerMessage(Level = LogLevel.Information, Message = "Synced metrics for {Platform} connection {ConnectionId}")]
    private static partial void LogMetricsSynced(ILogger logger, PlatformType platform, Guid connectionId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to sync {Platform}")]
    private static partial void LogSyncError(ILogger logger, PlatformType platform, Exception ex);
}
