using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.SharedProjects.Commands.CloseProject;

/// <summary>
/// Handler for closing a shared project.
/// Story: ACF-013
/// </summary>
public sealed partial class CloseProjectCommandHandler : IRequestHandler<CloseProjectCommand>
{
    private readonly ISharedProjectRepository _repository;
    private readonly ILogger<CloseProjectCommandHandler> _logger;

    public CloseProjectCommandHandler(
        ISharedProjectRepository repository,
        ILogger<CloseProjectCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(CloseProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            throw new SharedProjectNotFoundException(request.ProjectId);

        project.Close(request.UserId);

        await _repository.UpdateAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogProjectClosed(_logger, request.ProjectId, request.UserId);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Project {ProjectId} closed by user {UserId}")]
    private static partial void LogProjectClosed(ILogger logger, Guid projectId, Guid userId);
}
