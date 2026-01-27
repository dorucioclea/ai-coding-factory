using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Approvals.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Approvals.Commands.ConfigureWorkflow;

/// <summary>
/// Handler for ConfigureWorkflowCommand.
/// Story: ACF-009
/// </summary>
public sealed partial class ConfigureWorkflowCommandHandler : IRequestHandler<ConfigureWorkflowCommand, WorkflowSettingsResponse>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<ConfigureWorkflowCommandHandler> _logger;

    public ConfigureWorkflowCommandHandler(
        ITeamRepository teamRepository,
        ILogger<ConfigureWorkflowCommandHandler> logger)
    {
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<WorkflowSettingsResponse> Handle(ConfigureWorkflowCommand request, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new TeamNotFoundException(request.TeamId);
        }

        team.ConfigureWorkflow(request.RequiresApproval, request.ApproverIds, request.UserId);

        await _teamRepository.UpdateAsync(team, cancellationToken);
        await _teamRepository.SaveChangesAsync(cancellationToken);

        LogWorkflowConfigured(_logger, request.TeamId, request.RequiresApproval, request.UserId);

        return new WorkflowSettingsResponse
        {
            TeamId = team.Id,
            RequiresApproval = team.RequiresApproval,
            ApproverIds = team.ApproverIds.ToList()
        };
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Workflow configured for team {TeamId}: RequiresApproval={RequiresApproval} by user {UserId}")]
    private static partial void LogWorkflowConfigured(ILogger logger, Guid teamId, bool requiresApproval, Guid userId);
}
