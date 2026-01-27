using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Queries.GetProfileByUsername;

/// <summary>
/// Handler for GetProfileByUsernameQuery.
/// Story: ACF-002
/// </summary>
public sealed partial class GetProfileByUsernameQueryHandler : IRequestHandler<GetProfileByUsernameQuery, PublicProfileResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ICreatorProfileRepository _profileRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetProfileByUsernameQueryHandler> _logger;

    public GetProfileByUsernameQueryHandler(
        ICreatorProfileRepository profileRepository,
        ICacheService cacheService,
        ILogger<GetProfileByUsernameQueryHandler> logger)
    {
        _profileRepository = profileRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<PublicProfileResponse> Handle(GetProfileByUsernameQuery request, CancellationToken cancellationToken)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var cacheKey = $"profile:username:{normalizedUsername}";

        // Try to get from cache first
        var cachedProfile = await _cacheService.GetAsync<PublicProfileResponse>(cacheKey, cancellationToken);
        if (cachedProfile is not null)
        {
            LogCacheHit(_logger, normalizedUsername);
            return cachedProfile;
        }

        // Get from database
        var profile = await _profileRepository.GetByUsernameAsync(normalizedUsername, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException("CreatorProfile", normalizedUsername);
        }

        var response = PublicProfileResponse.FromEntity(profile);

        // Cache the response
        await _cacheService.SetAsync(cacheKey, response, CacheDuration, cancellationToken);

        LogCacheMiss(_logger, normalizedUsername);

        return response;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache hit for profile username: {Username}")]
    private static partial void LogCacheHit(ILogger logger, string username);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache miss for profile username: {Username}")]
    private static partial void LogCacheMiss(ILogger logger, string username);
}
