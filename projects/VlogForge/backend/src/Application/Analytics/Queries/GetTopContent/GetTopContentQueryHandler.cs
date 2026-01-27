using MediatR;
using VlogForge.Application.Analytics.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Analytics.Queries.GetTopContent;

/// <summary>
/// Handler for GetTopContentQuery.
/// Story: ACF-004
/// </summary>
public sealed class GetTopContentQueryHandler
    : IRequestHandler<GetTopContentQuery, TopContentResponse>
{
    private const string CacheVersion = "v1";
    private const string CacheKeyPrefix = $"analytics:content:{CacheVersion}:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    private readonly IContentPerformanceRepository _contentRepository;
    private readonly ICacheService _cacheService;

    public GetTopContentQueryHandler(
        IContentPerformanceRepository contentRepository,
        ICacheService cacheService)
    {
        _contentRepository = contentRepository;
        _cacheService = cacheService;
    }

    public async Task<TopContentResponse> Handle(
        GetTopContentQuery request,
        CancellationToken cancellationToken)
    {
        var sortBy = NormalizeSortBy(request.SortBy);
        var cacheKey = $"{CacheKeyPrefix}{request.UserId}:{sortBy}:{request.Limit}";

        // Try to get from cache first
        var cached = await _cacheService.GetAsync<TopContentResponse>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Get top content from repository
        var topContent = await _contentRepository.GetTopContentAsync(
            request.UserId,
            request.Limit,
            sortBy,
            cancellationToken);

        // Map to DTOs
        var contentDtos = topContent
            .Select(MapToDto)
            .ToList();

        var response = new TopContentResponse(contentDtos, sortBy);

        // Cache the response
        await _cacheService.SetAsync(cacheKey, response, CacheTtl, cancellationToken);

        return response;
    }

    private static string NormalizeSortBy(string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "views" => "views",
            "engagement" => "engagement",
            "likes" => "likes",
            "comments" => "comments",
            _ => "views" // Default to views
        };
    }

    private static ContentPerformanceDto MapToDto(ContentPerformance content)
    {
        return new ContentPerformanceDto(
            content.ContentId,
            content.PlatformType.ToString(),
            content.Title,
            content.ThumbnailUrl,
            content.ContentUrl,
            content.PublishedAt,
            content.ViewCount,
            content.LikeCount,
            content.CommentCount,
            Math.Round(content.EngagementRate, 2));
    }
}
