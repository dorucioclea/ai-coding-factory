using MediatR;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Commands.CreateProfile;

/// <summary>
/// Command to create a new creator profile.
/// Story: ACF-002
/// </summary>
public sealed record CreateProfileCommand(
    Guid UserId,
    string Username,
    string DisplayName,
    string? Bio = null
) : IRequest<CreatorProfileResponse>;
