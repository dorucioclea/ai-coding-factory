using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Discovery.DTOs;

namespace VlogForge.Application.Discovery.Queries.DiscoverCreators;

/// <summary>
/// Handler for DiscoverCreatorsQuery.
/// Story: ACF-010
/// </summary>
public sealed class DiscoverCreatorsQueryHandler : IRequestHandler<DiscoverCreatorsQuery, DiscoveryResponse>
{
    private readonly ICreatorProfileRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DiscoverCreatorsQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "discovery";

    public DiscoverCreatorsQueryHandler(
        ICreatorProfileRepository repository,
        ICacheService cacheService,
        ILogger<DiscoverCreatorsQueryHandler> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<DiscoveryResponse> Handle(DiscoverCreatorsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(request);

        // Try to get from cache first
        var cached = await _cacheService.GetAsync<DiscoveryResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Discovery results retrieved from cache for key: {CacheKey}", cacheKey);
            return cached;
        }

        // Convert audience size to follower ranges
        var (minFollowers, maxFollowers) = GetFollowerRange(request.AudienceSize);

        // Parse cursor if provided
        Guid? cursorId = null;
        if (!string.IsNullOrWhiteSpace(request.Cursor) && Guid.TryParse(request.Cursor, out var parsedCursor))
        {
            cursorId = parsedCursor;
        }

        // Normalize niches for case-insensitive matching
        var normalizedNiches = request.Niches?
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        var (profiles, hasMore, totalCount) = await _repository.DiscoverCreatorsAsync(
            excludeUserId: request.ExcludeUserId,
            niches: normalizedNiches,
            platforms: request.Platforms,
            minFollowers: minFollowers,
            maxFollowers: maxFollowers,
            searchTerm: request.SearchTerm,
            openToCollaboration: request.OpenToCollaboration,
            cursor: cursorId,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var items = profiles.Select(DiscoveryCreatorDto.FromEntity).ToList();

        // Determine next cursor (last item's ID if there are more results)
        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            nextCursor = items[^1].Id.ToString();
        }

        var response = new DiscoveryResponse
        {
            Items = items,
            TotalCount = totalCount,
            NextCursor = nextCursor,
            HasMore = hasMore,
            PageSize = request.PageSize
        };

        // Cache the response
        await _cacheService.SetAsync(cacheKey, response, CacheDuration, cancellationToken);
        _logger.LogDebug("Discovery results cached with key: {CacheKey}", cacheKey);

        return response;
    }

    private static string BuildCacheKey(DiscoverCreatorsQuery request)
    {
        var parts = new List<string> { CacheKeyPrefix };

        if (request.ExcludeUserId.HasValue)
        {
            parts.Add($"ex:{request.ExcludeUserId.Value}");
        }

        if (request.Niches is { Count: > 0 })
        {
            parts.Add($"n:{string.Join(",", request.Niches.OrderBy(n => n))}");
        }

        if (request.Platforms is { Count: > 0 })
        {
            parts.Add($"p:{string.Join(",", request.Platforms.OrderBy(p => p))}");
        }

        if (request.AudienceSize.HasValue)
        {
            parts.Add($"a:{request.AudienceSize.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            parts.Add($"s:{request.SearchTerm.ToLowerInvariant()}");
        }

        if (request.OpenToCollaboration.HasValue)
        {
            parts.Add($"c:{request.OpenToCollaboration.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            parts.Add($"cur:{request.Cursor}");
        }

        parts.Add($"ps:{request.PageSize}");

        return string.Join(":", parts);
    }

    private static (int? Min, int? Max) GetFollowerRange(AudienceSizeRange? audienceSize)
    {
        return audienceSize switch
        {
            AudienceSizeRange.Small => (1000, 10000),
            AudienceSizeRange.Medium => (10000, 100000),
            AudienceSizeRange.Large => (100000, null),
            _ => (null, null)
        };
    }
}
