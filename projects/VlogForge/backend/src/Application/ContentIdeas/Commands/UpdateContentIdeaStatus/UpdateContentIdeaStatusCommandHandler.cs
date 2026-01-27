using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.ContentIdeas.Commands.UpdateContentIdeaStatus;

/// <summary>
/// Handler for UpdateContentIdeaStatusCommand.
/// Story: ACF-005
/// </summary>
public sealed partial class UpdateContentIdeaStatusCommandHandler : IRequestHandler<UpdateContentIdeaStatusCommand, ContentIdeaResponse>
{
    private readonly IContentItemRepository _repository;
    private readonly ILogger<UpdateContentIdeaStatusCommandHandler> _logger;

    public UpdateContentIdeaStatusCommandHandler(
        IContentItemRepository repository,
        ILogger<UpdateContentIdeaStatusCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ContentIdeaResponse> Handle(UpdateContentIdeaStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);

        if (item is null)
        {
            throw new NotFoundException("ContentItem", request.Id);
        }

        // Verify ownership
        if (item.UserId != request.UserId)
        {
            throw new ForbiddenAccessException();
        }

        // Update status
        item.UpdateStatus(request.NewStatus);

        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogStatusUpdated(_logger, item.Id, request.NewStatus);

        return ContentIdeaResponse.FromEntity(item);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Updated content idea {ContentItemId} status to {NewStatus}")]
    private static partial void LogStatusUpdated(ILogger logger, Guid contentItemId, Domain.Entities.IdeaStatus newStatus);
}
