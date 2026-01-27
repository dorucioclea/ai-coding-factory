using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Profiles.Commands.CreateProfile;

/// <summary>
/// Handler for CreateProfileCommand.
/// Story: ACF-002
/// </summary>
public sealed partial class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, CreatorProfileResponse>
{
    private readonly ICreatorProfileRepository _profileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateProfileCommandHandler> _logger;

    public CreateProfileCommandHandler(
        ICreatorProfileRepository profileRepository,
        IUserRepository userRepository,
        ILogger<CreateProfileCommandHandler> logger)
    {
        _profileRepository = profileRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CreatorProfileResponse> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Check if user already has a profile
        if (await _profileRepository.UserHasProfileAsync(request.UserId, cancellationToken))
        {
            throw new ConflictException("CreatorProfile", request.UserId, "User already has a profile.");
        }

        // Check if username is taken
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        if (await _profileRepository.UsernameExistsAsync(normalizedUsername, cancellationToken))
        {
            throw new ConflictException("Username", request.Username, "This username is already taken.");
        }

        // Create the profile
        var profile = CreatorProfile.Create(request.UserId, request.Username, request.DisplayName);

        // Set bio if provided
        if (!string.IsNullOrWhiteSpace(request.Bio))
        {
            var bio = Bio.Create(request.Bio);
            profile.UpdateBasicInfo(request.DisplayName, bio);
        }

        // Persist
        await _profileRepository.AddAsync(profile, cancellationToken);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        LogProfileCreated(_logger, profile.Id, profile.Username);

        return CreatorProfileResponse.FromEntity(profile);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Created profile {ProfileId} with username {Username}")]
    private static partial void LogProfileCreated(ILogger logger, Guid profileId, string username);
}
