using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Queries.GetContentIdeas;

/// <summary>
/// Query to get content ideas for a user with optional filtering.
/// Story: ACF-005
/// </summary>
public sealed record GetContentIdeasQuery(
    Guid UserId,
    IdeaStatus? Status = null,
    string? PlatformTag = null,
    string? SearchTerm = null
) : IRequest<ContentIdeasListResponse>;
