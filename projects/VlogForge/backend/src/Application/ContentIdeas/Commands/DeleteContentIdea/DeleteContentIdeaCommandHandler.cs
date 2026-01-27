using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.ContentIdeas.Commands.DeleteContentIdea;

/// <summary>
/// Handler for DeleteContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed partial class DeleteContentIdeaCommandHandler : IRequestHandler<DeleteContentIdeaCommand, bool>
{
    private readonly IContentItemRepository _repository;
    private readonly ILogger<DeleteContentIdeaCommandHandler> _logger;

    public DeleteContentIdeaCommandHandler(
        IContentItemRepository repository,
        ILogger<DeleteContentIdeaCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteContentIdeaCommand request, CancellationToken cancellationToken)
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

        // Soft delete
        item.SoftDelete();

        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogContentIdeaDeleted(_logger, item.Id);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Soft-deleted content idea {ContentItemId}")]
    private static partial void LogContentIdeaDeleted(ILogger logger, Guid contentItemId);
}
