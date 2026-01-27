using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.DTOs;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Profiles.Commands.UpdateProfile;

/// <summary>
/// Handler for UpdateProfileCommand.
/// Story: ACF-002
/// </summary>
public sealed partial class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, CreatorProfileResponse>
{
    private readonly ICreatorProfileRepository _profileRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        ICreatorProfileRepository profileRepository,
        ICacheService cacheService,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _profileRepository = profileRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CreatorProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // Get the profile
        var profile = await _profileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException("CreatorProfile", request.UserId);
        }

        // Update basic info
        var bio = request.Bio is not null ? Bio.Create(request.Bio) : Bio.Empty;
        profile.UpdateBasicInfo(request.DisplayName, bio);

        // Update niche tags if provided
        if (request.NicheTags is not null)
        {
            var tags = request.NicheTags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => NicheTag.Create(t))
                .ToList();
            profile.SetNicheTags(tags);
        }

        // Update collaboration settings if provided
        if (request.OpenToCollaborations.HasValue)
        {
            profile.SetCollaborationSettings(request.OpenToCollaborations.Value, request.CollaborationPreferences);
        }

        // Persist
        await _profileRepository.UpdateAsync(profile, cancellationToken);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"profile:username:{profile.Username}", cancellationToken);

        LogProfileUpdated(_logger, profile.Id, profile.Username);

        return CreatorProfileResponse.FromEntity(profile);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Updated profile {ProfileId} (username: {Username})")]
    private static partial void LogProfileUpdated(ILogger logger, Guid profileId, string username);
}
