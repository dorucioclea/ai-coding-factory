using MediatR;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.ContentIdeas.Queries.GetContentIdeaById;

/// <summary>
/// Handler for GetContentIdeaByIdQuery.
/// Story: ACF-005
/// </summary>
public sealed class GetContentIdeaByIdQueryHandler : IRequestHandler<GetContentIdeaByIdQuery, ContentIdeaResponse?>
{
    private readonly IContentItemRepository _repository;

    public GetContentIdeaByIdQueryHandler(IContentItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContentIdeaResponse?> Handle(GetContentIdeaByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);

        if (item is null)
        {
            return null;
        }

        // Verify ownership
        if (item.UserId != request.UserId)
        {
            throw new ForbiddenAccessException();
        }

        return ContentIdeaResponse.FromEntity(item);
    }
}
