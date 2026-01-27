using MediatR;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Commands.UpdateProfile;

/// <summary>
/// Command to update a creator profile.
/// Story: ACF-002
/// </summary>
public sealed record UpdateProfileCommand(
    Guid UserId,
    string DisplayName,
    string? Bio,
    IReadOnlyList<string>? NicheTags,
    bool? OpenToCollaborations,
    string? CollaborationPreferences
) : IRequest<CreatorProfileResponse>;
