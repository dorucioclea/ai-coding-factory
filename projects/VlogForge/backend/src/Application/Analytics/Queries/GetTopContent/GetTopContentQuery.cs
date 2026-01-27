using MediatR;
using VlogForge.Application.Analytics.DTOs;

namespace VlogForge.Application.Analytics.Queries.GetTopContent;

/// <summary>
/// Query to get top performing content.
/// Story: ACF-004 (AC4)
/// </summary>
public sealed record GetTopContentQuery(
    Guid UserId,
    int Limit = 10,
    string SortBy = "views") : IRequest<TopContentResponse>;
