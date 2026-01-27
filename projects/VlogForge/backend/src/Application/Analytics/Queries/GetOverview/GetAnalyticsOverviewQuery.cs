using MediatR;
using VlogForge.Application.Analytics.DTOs;

namespace VlogForge.Application.Analytics.Queries.GetOverview;

/// <summary>
/// Query to get analytics overview for the current user.
/// Story: ACF-004 (AC1, AC3, AC5)
/// </summary>
public sealed record GetAnalyticsOverviewQuery(Guid UserId) : IRequest<AnalyticsOverviewResponse>;
