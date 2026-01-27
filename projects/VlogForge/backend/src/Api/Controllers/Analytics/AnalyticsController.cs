using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Application.Analytics.DTOs;
using VlogForge.Application.Analytics.Queries.GetOverview;
using VlogForge.Application.Analytics.Queries.GetTopContent;
using VlogForge.Application.Analytics.Queries.GetTrends;

namespace VlogForge.Api.Controllers.Analytics;

/// <summary>
/// Analytics dashboard endpoints for viewing metrics and trends.
/// Story: ACF-004
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the analytics overview including total metrics and platform breakdown.
    /// </summary>
    /// <returns>Overview metrics with growth indicators.</returns>
    /// <response code="200">Returns the analytics overview.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(AnalyticsOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AnalyticsOverviewResponse>> GetOverview()
    {
        var userId = GetCurrentUserId();
        var query = new GetAnalyticsOverviewQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets analytics trends for a specified time period.
    /// </summary>
    /// <param name="period">Time period: 7d, 30d, or 90d (default: 7d).</param>
    /// <returns>Trend data for charts.</returns>
    /// <response code="200">Returns the analytics trends.</response>
    /// <response code="400">If the period is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(AnalyticsTrendsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AnalyticsTrendsResponse>> GetTrends(
        [FromQuery] string period = "7d")
    {
        if (!IsValidPeriod(period))
        {
            return BadRequest($"Invalid period: {period}. Must be 7d, 30d, or 90d.");
        }

        var userId = GetCurrentUserId();
        var query = new GetAnalyticsTrendsQuery(userId, period);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets top performing content across all platforms.
    /// </summary>
    /// <param name="sortBy">Sort by: views, engagement, likes, or comments (default: views).</param>
    /// <param name="limit">Number of items to return (default: 10, max: 50).</param>
    /// <returns>List of top performing content.</returns>
    /// <response code="200">Returns the top content list.</response>
    /// <response code="400">If parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("top-content")]
    [ProducesResponseType(typeof(TopContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TopContentResponse>> GetTopContent(
        [FromQuery] string sortBy = "views",
        [FromQuery] int limit = 10)
    {
        if (!IsValidSortBy(sortBy))
        {
            return BadRequest($"Invalid sortBy: {sortBy}. Must be views, engagement, likes, or comments.");
        }

        if (limit < 1 || limit > 50)
        {
            return BadRequest("Limit must be between 1 and 50.");
        }

        var userId = GetCurrentUserId();
        var query = new GetTopContentQuery(userId, limit, sortBy);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
        return userId;
    }

    private static bool IsValidPeriod(string period)
    {
        return period.ToLowerInvariant() is "7d" or "30d" or "90d";
    }

    private static bool IsValidSortBy(string sortBy)
    {
        return sortBy.ToLowerInvariant() is "views" or "engagement" or "likes" or "comments";
    }
}
