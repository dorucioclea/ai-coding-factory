using MediatR;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Commands.SetCollaborationSettings;

/// <summary>
/// Command to set collaboration availability settings.
/// Story: ACF-002
/// </summary>
public sealed record SetCollaborationSettingsCommand(
    Guid UserId,
    bool OpenToCollaborations,
    string? CollaborationPreferences
) : IRequest<CreatorProfileResponse>;
