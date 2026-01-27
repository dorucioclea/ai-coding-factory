using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Commands.SetCollaborationSettings;

/// <summary>
/// Handler for SetCollaborationSettingsCommand.
/// Story: ACF-002
/// </summary>
public sealed partial class SetCollaborationSettingsCommandHandler : IRequestHandler<SetCollaborationSettingsCommand, CreatorProfileResponse>
{
    private readonly ICreatorProfileRepository _profileRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SetCollaborationSettingsCommandHandler> _logger;

    public SetCollaborationSettingsCommandHandler(
        ICreatorProfileRepository profileRepository,
        ICacheService cacheService,
        ILogger<SetCollaborationSettingsCommandHandler> logger)
    {
        _profileRepository = profileRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CreatorProfileResponse> Handle(SetCollaborationSettingsCommand request, CancellationToken cancellationToken)
    {
        // Get the profile
        var profile = await _profileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException("CreatorProfile", request.UserId);
        }

        // Update collaboration settings
        profile.SetCollaborationSettings(request.OpenToCollaborations, request.CollaborationPreferences);

        // Persist
        await _profileRepository.UpdateAsync(profile, cancellationToken);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"profile:username:{profile.Username}", cancellationToken);

        LogCollaborationSettingsUpdated(_logger, profile.Id, request.OpenToCollaborations);

        return CreatorProfileResponse.FromEntity(profile);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Updated collaboration settings for profile {ProfileId}: OpenToCollaborations={OpenToCollaborations}")]
    private static partial void LogCollaborationSettingsUpdated(ILogger logger, Guid profileId, bool openToCollaborations);
}
