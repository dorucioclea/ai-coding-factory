using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.ContentIdeas.Commands.UpdateContentIdea;

/// <summary>
/// Handler for UpdateContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed partial class UpdateContentIdeaCommandHandler : IRequestHandler<UpdateContentIdeaCommand, ContentIdeaResponse>
{
    private readonly IContentItemRepository _repository;
    private readonly ILogger<UpdateContentIdeaCommandHandler> _logger;

    public UpdateContentIdeaCommandHandler(
        IContentItemRepository repository,
        ILogger<UpdateContentIdeaCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ContentIdeaResponse> Handle(UpdateContentIdeaCommand request, CancellationToken cancellationToken)
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

        // Update title and notes
        item.Update(request.Title, request.Notes);

        // Update platform tags if provided
        if (request.PlatformTags is not null)
        {
            item.SetPlatformTags(request.PlatformTags);
        }

        // Update status if provided
        if (request.Status.HasValue)
        {
            item.UpdateStatus(request.Status.Value);
        }

        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogContentIdeaUpdated(_logger, item.Id);

        return ContentIdeaResponse.FromEntity(item);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Updated content idea {ContentItemId}")]
    private static partial void LogContentIdeaUpdated(ILogger logger, Guid contentItemId);
}
