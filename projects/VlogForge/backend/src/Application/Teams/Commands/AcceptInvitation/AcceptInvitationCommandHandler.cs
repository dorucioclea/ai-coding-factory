using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Commands.AcceptInvitation;

/// <summary>
/// Handler for AcceptInvitationCommand.
/// Story: ACF-007
/// </summary>
public sealed partial class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, TeamMemberResponse>
{
    private readonly ITeamRepository _repository;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    public AcceptInvitationCommandHandler(
        ITeamRepository repository,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TeamMemberResponse> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByInvitationTokenAsync(request.Token, cancellationToken);
        if (team is null)
        {
            throw new InvalidOperationException("Invitation not found or has expired.");
        }

        // Accept invitation (domain validates expiration, email match, and membership)
        team.AcceptInvitation(request.Token, request.UserId, request.UserEmail);

        await _repository.UpdateAsync(team, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var member = team.GetMember(request.UserId)!;

        LogInvitationAccepted(_logger, team.Id, request.UserId, member.Role.ToString());

        return TeamMemberResponse.FromEntity(member);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} accepted invitation to team {TeamId} with role {Role}")]
    private static partial void LogInvitationAccepted(ILogger logger, Guid teamId, Guid userId, string role);
}
