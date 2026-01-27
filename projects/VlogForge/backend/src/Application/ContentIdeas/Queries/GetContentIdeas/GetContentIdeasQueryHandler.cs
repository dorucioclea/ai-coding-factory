using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Queries.GetContentIdeas;

/// <summary>
/// Handler for GetContentIdeasQuery.
/// Story: ACF-005
/// </summary>
public sealed class GetContentIdeasQueryHandler : IRequestHandler<GetContentIdeasQuery, ContentIdeasListResponse>
{
    private readonly IContentItemRepository _repository;

    public GetContentIdeasQueryHandler(IContentItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContentIdeasListResponse> Handle(GetContentIdeasQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ContentItem> items;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            items = await _repository.SearchAsync(request.UserId, request.SearchTerm, cancellationToken);
        }
        else if (request.Status.HasValue || !string.IsNullOrWhiteSpace(request.PlatformTag))
        {
            items = await _repository.GetByUserIdWithFiltersAsync(
                request.UserId,
                request.Status,
                request.PlatformTag,
                cancellationToken);
        }
        else
        {
            items = await _repository.GetByUserIdAsync(request.UserId, cancellationToken: cancellationToken);
        }

        // Items are already sorted by CreatedAt DESC from the repository
        return new ContentIdeasListResponse
        {
            Items = items.Select(ContentIdeaResponse.FromEntity).ToList(),
            TotalCount = items.Count
        };
    }
}
