using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Commands.CreateContentIdea;

/// <summary>
/// Handler for CreateContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed partial class CreateContentIdeaCommandHandler : IRequestHandler<CreateContentIdeaCommand, ContentIdeaResponse>
{
    private readonly IContentItemRepository _repository;
    private readonly ILogger<CreateContentIdeaCommandHandler> _logger;

    public CreateContentIdeaCommandHandler(
        IContentItemRepository repository,
        ILogger<CreateContentIdeaCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ContentIdeaResponse> Handle(CreateContentIdeaCommand request, CancellationToken cancellationToken)
    {
        // Create the content item
        var item = ContentItem.Create(request.UserId, request.Title, request.Notes);

        // Add platform tags if provided
        if (request.PlatformTags is { Count: > 0 })
        {
            item.SetPlatformTags(request.PlatformTags);
        }

        // Persist
        await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogContentIdeaCreated(_logger, item.Id, item.Title);

        return ContentIdeaResponse.FromEntity(item);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Created content idea {ContentItemId} with title '{Title}'")]
    private static partial void LogContentIdeaCreated(ILogger logger, Guid contentItemId, string title);
}
