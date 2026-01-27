using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Commands.InviteMember;

/// <summary>
/// Handler for InviteMemberCommand.
/// Story: ACF-007
/// </summary>
public sealed partial class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, TeamInvitationResponse>
{
    private readonly ITeamRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<InviteMemberCommandHandler> _logger;

    public InviteMemberCommandHandler(
        ITeamRepository repository,
        IEmailService emailService,
        ILogger<InviteMemberCommandHandler> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<TeamInvitationResponse> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdWithInvitationsAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new InvalidOperationException("Team not found.");
        }

        // Create invitation (domain validates permissions)
        var token = team.InviteMember(request.Email, request.Role, request.InvitedByUserId);

        await _repository.UpdateAsync(team, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var invitation = team.Invitations.First(i => i.Token == token);

        // Send invitation email with proper error handling
        await SendInvitationEmailAsync(team.Name, request.Email, token, cancellationToken);

        LogMemberInvited(_logger, request.TeamId, request.Email, request.Role.ToString());

        return TeamInvitationResponse.FromEntity(invitation);
    }

    private async Task SendInvitationEmailAsync(string teamName, string email, string token, CancellationToken cancellationToken)
    {
        try
        {
            await _emailService.SendTeamInvitationAsync(email, teamName, token, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the failure but don't fail the invitation creation
            // The invitation exists in DB - admin can resend or user can request new invite
            LogEmailFailed(_logger, email, ex);
            // Note: In production, consider using an outbox pattern for reliable email delivery
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Invited {Email} to team {TeamId} with role {Role}")]
    private static partial void LogMemberInvited(ILogger logger, Guid teamId, string email, string role);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to send invitation email to {Email}")]
    private static partial void LogEmailFailed(ILogger logger, string email, Exception ex);
}
