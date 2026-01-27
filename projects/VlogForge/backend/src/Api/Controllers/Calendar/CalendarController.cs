using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Application.Calendar.DTOs;
using VlogForge.Application.Calendar.Queries.GetCalendar;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Api.Controllers.Calendar;

/// <summary>
/// Calendar view endpoints for content scheduling.
/// Story: ACF-006
/// </summary>
[ApiController]
[Route("api/calendar")]
[Authorize]
[Produces("application/json")]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CalendarController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets calendar data for a specific month.
    /// </summary>
    /// <param name="month">Month in YYYY-MM format.</param>
    /// <returns>Calendar data with scheduled content items.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CalendarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CalendarResponse>> GetCalendar([FromQuery] string month)
    {
        if (string.IsNullOrWhiteSpace(month) || !TryParseYearMonth(month, out var year, out var monthNumber))
        {
            return BadRequest("Invalid month format. Expected YYYY-MM.");
        }

        var userId = GetCurrentUserId();
        var query = new GetCalendarQuery(userId, year, monthNumber);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private static bool TryParseYearMonth(string input, out int year, out int month)
    {
        year = 0;
        month = 0;

        var parts = input.Split('-');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out year) || !int.TryParse(parts[1], out month))
            return false;

        return year >= 2020 && year <= 2100 && month >= 1 && month <= 12;
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
        return userId;
    }
}
