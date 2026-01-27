using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Teams.Commands.CreateTeam;

/// <summary>
/// Handler for CreateTeamCommand.
/// Story: ACF-007
/// </summary>
public sealed partial class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, TeamResponse>
{
    private readonly ITeamRepository _repository;
    private readonly ILogger<CreateTeamCommandHandler> _logger;

    public CreateTeamCommandHandler(
        ITeamRepository repository,
        ILogger<CreateTeamCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TeamResponse> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        // Check if team name already exists - the DB unique constraint is the authoritative check
        // but we do an upfront check for better error messages in the common case
        if (await _repository.ExistsWithNameAsync(request.OwnerId, request.Name, cancellationToken))
        {
            throw new TeamNameAlreadyExistsException();
        }

        // Create the team
        var team = Team.Create(request.OwnerId, request.Name, request.Description);

        // Persist - in rare race conditions, DB unique constraint will throw
        // which propagates as an exception that can be caught by global exception handler
        await _repository.AddAsync(team, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogTeamCreated(_logger, team.Id, team.Name, request.OwnerId);

        return TeamResponse.FromEntity(team);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Created team {TeamId} with name '{TeamName}' for owner {OwnerId}")]
    private static partial void LogTeamCreated(ILogger logger, Guid teamId, string teamName, Guid ownerId);
}
