using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Calendar.Commands.UpdateScheduledDate;

/// <summary>
/// Handler for UpdateScheduledDateCommand.
/// Story: ACF-006
/// </summary>
public sealed partial class UpdateScheduledDateCommandHandler : IRequestHandler<UpdateScheduledDateCommand, ContentIdeaResponse>
{
    private readonly IContentItemRepository _repository;
    private readonly ILogger<UpdateScheduledDateCommandHandler> _logger;

    public UpdateScheduledDateCommandHandler(
        IContentItemRepository repository,
        ILogger<UpdateScheduledDateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ContentIdeaResponse> Handle(UpdateScheduledDateCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);

        if (item == null)
        {
            throw new NotFoundException(nameof(ContentItem), request.Id);
        }

        if (item.UserId != request.UserId)
        {
            throw new ForbiddenAccessException();
        }

        if (request.ScheduledDate.HasValue)
        {
            item.UpdateScheduledDate(request.ScheduledDate.Value);
            LogScheduledDateUpdated(_logger, item.Id, request.ScheduledDate.Value);
        }
        else
        {
            item.ClearScheduledDate();
            LogScheduledDateCleared(_logger, item.Id);
        }

        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ContentIdeaResponse.FromEntity(item);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Updated scheduled date for content item {ContentItemId} to {ScheduledDate}")]
    private static partial void LogScheduledDateUpdated(ILogger logger, Guid contentItemId, DateTime scheduledDate);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cleared scheduled date for content item {ContentItemId}")]
    private static partial void LogScheduledDateCleared(ILogger logger, Guid contentItemId);
}
