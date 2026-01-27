using MediatR;
using VlogForge.Application.Calendar.DTOs;

namespace VlogForge.Application.Calendar.Queries.GetCalendar;

/// <summary>
/// Query to get calendar data for a specific month.
/// Story: ACF-006
/// </summary>
public sealed record GetCalendarQuery(
    Guid UserId,
    int Year,
    int Month
) : IRequest<CalendarResponse>;
