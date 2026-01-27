using MediatR;
using VlogForge.Application.Discovery.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Discovery.Queries.DiscoverCreators;

/// <summary>
/// Query to discover creators with filtering and cursor-based pagination.
/// Story: ACF-010
/// </summary>
public sealed record DiscoverCreatorsQuery(
    Guid? ExcludeUserId = null,
    IReadOnlyList<string>? Niches = null,
    IReadOnlyList<PlatformType>? Platforms = null,
    AudienceSizeRange? AudienceSize = null,
    string? SearchTerm = null,
    bool? OpenToCollaboration = null,
    string? Cursor = null,
    int PageSize = 20
) : IRequest<DiscoveryResponse>;
