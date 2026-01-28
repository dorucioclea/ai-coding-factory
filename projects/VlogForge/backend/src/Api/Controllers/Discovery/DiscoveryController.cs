using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Discovery.DTOs;
using VlogForge.Application.Discovery.Queries.DiscoverCreators;
using VlogForge.Domain.Entities;

namespace VlogForge.Api.Controllers.Discovery;

/// <summary>
/// Creator discovery endpoints for finding collaboration partners.
/// Story: ACF-010
/// </summary>
[ApiController]
[Route("api/discovery")]
[Produces("application/json")]
public class DiscoveryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DiscoveryController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Discovers creators with optional filtering and pagination.
    /// </summary>
    /// <param name="niches">Filter by niche tags (comma-separated).</param>
    /// <param name="platforms">Filter by platform types (comma-separated: YouTube, TikTok, Instagram).</param>
    /// <param name="audienceSize">Filter by audience range: Small (1K-10K), Medium (10K-100K), Large (100K+).</param>
    /// <param name="search">Search by name, username, or bio.</param>
    /// <param name="openToCollab">Filter to show only creators open to collaboration.</param>
    /// <param name="cursor">Cursor for pagination (ID of last item from previous page).</param>
    /// <param name="pageSize">Number of results per page (default: 20, max: 50).</param>
    /// <returns>Paginated list of discoverable creators.</returns>
    [HttpGet]
    [Authorize]
    [EnableRateLimiting("discovery")]
    [ProducesResponseType(typeof(DiscoveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DiscoveryResponse>> DiscoverCreators(
        [FromQuery] string? niches = null,
        [FromQuery] string? platforms = null,
        [FromQuery] AudienceSizeRange? audienceSize = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? openToCollab = null,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20)
    {
        // Validate page size
        if (pageSize < 1 || pageSize > 50)
        {
            return BadRequest("Page size must be between 1 and 50.");
        }

        // Parse niches
        List<string>? nicheList = null;
        if (!string.IsNullOrWhiteSpace(niches))
        {
            nicheList = niches.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .Take(10)
                .ToList();
        }

        // Parse platforms
        List<PlatformType>? platformList = null;
        if (!string.IsNullOrWhiteSpace(platforms))
        {
            platformList = new List<PlatformType>();
            var platformStrings = platforms.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var platformStr in platformStrings)
            {
                if (Enum.TryParse<PlatformType>(platformStr, ignoreCase: true, out var platform))
                {
                    if (!platformList.Contains(platform))
                    {
                        platformList.Add(platform);
                    }
                }
            }
            if (platformList.Count == 0)
            {
                platformList = null;
            }
        }

        // Get current user ID to exclude from results
        Guid? currentUserId = null;
        if (!string.IsNullOrEmpty(_currentUserService.UserId) &&
            Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            currentUserId = userId;
        }

        var query = new DiscoverCreatorsQuery(
            ExcludeUserId: currentUserId,
            Niches: nicheList,
            Platforms: platformList,
            AudienceSize: audienceSize,
            SearchTerm: search,
            OpenToCollaboration: openToCollab,
            Cursor: cursor,
            PageSize: pageSize);

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets available niche categories for filtering.
    /// </summary>
    /// <returns>List of common niche categories.</returns>
    [HttpGet("niches")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetNicheCategories()
    {
        // Return common niche categories for the UI
        var niches = new[]
        {
            "Gaming",
            "Lifestyle",
            "Tech",
            "Beauty",
            "Fashion",
            "Food",
            "Travel",
            "Fitness",
            "Music",
            "Comedy",
            "Education",
            "Vlog",
            "DIY",
            "Art",
            "Finance",
            "Sports",
            "News",
            "Entertainment",
            "Science",
            "Health"
        };

        return Ok(niches);
    }

    /// <summary>
    /// Gets available platform types for filtering.
    /// </summary>
    /// <returns>List of supported platforms.</returns>
    [HttpGet("platforms")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetPlatforms()
    {
        var platforms = Enum.GetNames<PlatformType>();
        return Ok(platforms);
    }
}
