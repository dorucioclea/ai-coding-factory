using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.ContentIdeas.Queries.GetContentIdeaById;

/// <summary>
/// Query to get a content idea by ID.
/// Story: ACF-005
/// </summary>
public sealed record GetContentIdeaByIdQuery(
    Guid Id,
    Guid UserId
) : IRequest<ContentIdeaResponse?>;
