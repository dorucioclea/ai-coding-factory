using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.SharedProjects.Commands.CreateSharedProject;

/// <summary>
/// Handler for creating a shared project.
/// Story: ACF-013
/// </summary>
public sealed partial class CreateSharedProjectCommandHandler
    : IRequestHandler<CreateSharedProjectCommand, SharedProjectResponse>
{
    private readonly ISharedProjectRepository _repository;
    private readonly ILogger<CreateSharedProjectCommandHandler> _logger;

    public CreateSharedProjectCommandHandler(
        ISharedProjectRepository repository,
        ILogger<CreateSharedProjectCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SharedProjectResponse> Handle(
        CreateSharedProjectCommand request,
        CancellationToken cancellationToken)
    {
        // Check if project already exists for this collaboration request
        var existing = await _repository.GetByCollaborationRequestIdAsync(
            request.CollaborationRequestId, cancellationToken);
        if (existing is not null)
            return SharedProjectResponse.FromEntity(existing);

        var project = SharedProject.Create(
            request.CollaborationRequestId,
            request.SenderId,
            request.RecipientId,
            request.Name,
            request.Description);

        await _repository.AddAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogProjectCreated(_logger, project.Id, project.Name, request.SenderId, request.RecipientId);

        return SharedProjectResponse.FromEntity(project);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Shared project {ProjectId} '{ProjectName}' created for users {SenderId} and {RecipientId}")]
    private static partial void LogProjectCreated(
        ILogger logger, Guid projectId, string projectName, Guid senderId, Guid recipientId);
}
