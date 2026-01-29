using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.SharedProjects.Commands.LeaveProject;

/// <summary>
/// Handler for leaving a shared project.
/// Story: ACF-013
/// </summary>
public sealed partial class LeaveProjectCommandHandler : IRequestHandler<LeaveProjectCommand>
{
    private readonly ISharedProjectRepository _repository;
    private readonly ILogger<LeaveProjectCommandHandler> _logger;

    public LeaveProjectCommandHandler(
        ISharedProjectRepository repository,
        ILogger<LeaveProjectCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(LeaveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            throw new SharedProjectNotFoundException(request.ProjectId);

        project.Leave(request.UserId);

        await _repository.UpdateAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogMemberLeft(_logger, request.ProjectId, request.UserId);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "User {UserId} left project {ProjectId}")]
    private static partial void LogMemberLeft(ILogger logger, Guid projectId, Guid userId);
}
