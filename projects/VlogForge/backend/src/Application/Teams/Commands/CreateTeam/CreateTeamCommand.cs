using MediatR;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Commands.CreateTeam;

/// <summary>
/// Command to create a new team.
/// Story: ACF-007
/// </summary>
public sealed record CreateTeamCommand(
    Guid OwnerId,
    string Name,
    string? Description = null
) : IRequest<TeamResponse>;
