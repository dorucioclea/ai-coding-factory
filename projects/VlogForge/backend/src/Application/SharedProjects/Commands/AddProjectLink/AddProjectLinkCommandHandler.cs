using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.SharedProjects.Commands.AddProjectLink;

/// <summary>
/// Handler for adding a link to a shared project.
/// Story: ACF-013
/// </summary>
public sealed partial class AddProjectLinkCommandHandler
    : IRequestHandler<AddProjectLinkCommand, SharedProjectLinkResponse>
{
    private readonly ISharedProjectRepository _repository;
    private readonly ILogger<AddProjectLinkCommandHandler> _logger;

    public AddProjectLinkCommandHandler(
        ISharedProjectRepository repository,
        ILogger<AddProjectLinkCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SharedProjectLinkResponse> Handle(
        AddProjectLinkCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            throw new SharedProjectNotFoundException(request.ProjectId);

        var link = project.AddLink(request.UserId, request.Title, request.Url, request.Description);

        await _repository.UpdateAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogLinkAdded(_logger, link.Id, request.ProjectId, request.UserId);

        return SharedProjectLinkResponse.FromEntity(link);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Link {LinkId} added to project {ProjectId} by user {UserId}")]
    private static partial void LogLinkAdded(ILogger logger, Guid linkId, Guid projectId, Guid userId);
}
