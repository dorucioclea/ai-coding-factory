using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Commands.UploadProfilePicture;

/// <summary>
/// Handler for UploadProfilePictureCommand.
/// Story: ACF-002
/// </summary>
public sealed partial class UploadProfilePictureCommandHandler : IRequestHandler<UploadProfilePictureCommand, CreatorProfileResponse>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly ICreatorProfileRepository _profileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UploadProfilePictureCommandHandler> _logger;

    public UploadProfilePictureCommandHandler(
        ICreatorProfileRepository profileRepository,
        IFileStorageService fileStorageService,
        ICacheService cacheService,
        ILogger<UploadProfilePictureCommandHandler> logger)
    {
        _profileRepository = profileRepository;
        _fileStorageService = fileStorageService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CreatorProfileResponse> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        // Validate content type
        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            throw new ValidationException("ContentType", $"Content type '{request.ContentType}' is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}");
        }

        // Validate file size
        if (request.ImageStream.Length > MaxFileSizeBytes)
        {
            throw new ValidationException("FileSize", $"File size exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB.");
        }

        // Get the profile
        var profile = await _profileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException("CreatorProfile", request.UserId);
        }

        // Delete old profile picture if exists
        if (!string.IsNullOrEmpty(profile.ProfilePictureUrl))
        {
            try
            {
                await _fileStorageService.DeleteAsync(profile.ProfilePictureUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                LogDeleteOldPictureFailed(_logger, ex, profile.ProfilePictureUrl);
                // Continue - don't fail upload because of cleanup issues
            }
        }

        // Upload new picture with variants
        var pictureUrls = await _fileStorageService.UploadProfilePictureAsync(
            request.ImageStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        // Update profile
        profile.UpdateProfilePicture(pictureUrls.StandardUrl);

        // Persist
        await _profileRepository.UpdateAsync(profile, cancellationToken);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"profile:username:{profile.Username}", cancellationToken);

        LogProfilePictureUploaded(_logger, profile.Id, pictureUrls.StandardUrl);

        return CreatorProfileResponse.FromEntity(profile);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to delete old profile picture: {Url}")]
    private static partial void LogDeleteOldPictureFailed(ILogger logger, Exception ex, string url);

    [LoggerMessage(Level = LogLevel.Information, Message = "Uploaded profile picture for profile {ProfileId}: {Url}")]
    private static partial void LogProfilePictureUploaded(ILogger logger, Guid profileId, string url);
}
