using MediatR;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Commands.UploadProfilePicture;

/// <summary>
/// Command to upload a profile picture.
/// Story: ACF-002
/// </summary>
public sealed record UploadProfilePictureCommand(
    Guid UserId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : IRequest<CreatorProfileResponse>;
