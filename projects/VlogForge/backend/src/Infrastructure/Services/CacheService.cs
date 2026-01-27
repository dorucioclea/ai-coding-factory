using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// Redis-based cache service implementation.
/// Story: ACF-002
/// </summary>
public sealed partial class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            LogCacheGetError(_logger, ex, key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var data = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(key, data, options, cancellationToken);
            LogCacheSet(_logger, key, expiration.TotalSeconds);
        }
        catch (Exception ex)
        {
            LogCacheSetError(_logger, ex, key);
            // Don't rethrow - caching failures shouldn't break the application
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            LogCacheRemove(_logger, key);
        }
        catch (Exception ex)
        {
            LogCacheRemoveError(_logger, ex, key);
            // Don't rethrow - caching failures shouldn't break the application
        }
    }

    /// <inheritdoc />
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal requires Redis SCAN command
        // which isn't available in IDistributedCache.
        // For production, consider using StackExchange.Redis directly
        // or a dedicated cache abstraction that supports patterns.
        LogCachePatternRemoveNotSupported(_logger, pattern);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get cache key: {Key}")]
    private static partial void LogCacheGetError(ILogger logger, Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache set: {Key} (TTL: {TtlSeconds}s)")]
    private static partial void LogCacheSet(ILogger logger, string key, double ttlSeconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to set cache key: {Key}")]
    private static partial void LogCacheSetError(ILogger logger, Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache removed: {Key}")]
    private static partial void LogCacheRemove(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to remove cache key: {Key}")]
    private static partial void LogCacheRemoveError(ILogger logger, Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Pattern-based cache removal not supported with IDistributedCache: {Pattern}")]
    private static partial void LogCachePatternRemoveNotSupported(ILogger logger, string pattern);
}
