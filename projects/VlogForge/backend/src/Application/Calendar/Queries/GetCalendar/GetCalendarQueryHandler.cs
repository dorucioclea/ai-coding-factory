using MediatR;
using VlogForge.Application.Calendar.DTOs;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Calendar.Queries.GetCalendar;

/// <summary>
/// Handler for GetCalendarQuery.
/// Story: ACF-006
/// </summary>
public sealed class GetCalendarQueryHandler : IRequestHandler<GetCalendarQuery, CalendarResponse>
{
    private readonly IContentItemRepository _repository;

    public GetCalendarQueryHandler(IContentItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<CalendarResponse> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var items = await _repository.GetScheduledForDateRangeAsync(
            request.UserId,
            startDate,
            endDate,
            cancellationToken);

        var groupedByDay = items
            .GroupBy(i => DateOnly.FromDateTime(i.ScheduledDate!.Value))
            .OrderBy(g => g.Key)
            .Select(g => new CalendarDayResponse
            {
                Date = g.Key,
                Items = g.Select(CalendarItemResponse.FromEntity).ToList()
            })
            .ToList();

        return new CalendarResponse
        {
            Year = request.Year,
            Month = request.Month,
            Days = groupedByDay,
            TotalItems = items.Count
        };
    }
}
