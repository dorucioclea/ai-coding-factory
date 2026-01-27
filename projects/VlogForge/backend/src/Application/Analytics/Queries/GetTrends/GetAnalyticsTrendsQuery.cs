using MediatR;
using VlogForge.Application.Analytics.DTOs;

namespace VlogForge.Application.Analytics.Queries.GetTrends;

/// <summary>
/// Query to get analytics trends for a time period.
/// Story: ACF-004 (AC2)
/// </summary>
public sealed record GetAnalyticsTrendsQuery(
    Guid UserId,
    string Period = "7d") : IRequest<AnalyticsTrendsResponse>;
