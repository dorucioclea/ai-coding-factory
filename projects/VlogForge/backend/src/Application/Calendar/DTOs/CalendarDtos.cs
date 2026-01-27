using VlogForge.Domain.Entities;

namespace VlogForge.Application.Calendar.DTOs;

/// <summary>
/// Response DTO for a calendar item (minimal data for calendar view).
/// Story: ACF-006
/// </summary>
public sealed class CalendarItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public IdeaStatus Status { get; init; }
    public DateTime ScheduledDate { get; init; }
    public IReadOnlyList<string> PlatformTags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a response from a ContentItem entity.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when item has no scheduled date.</exception>
    public static CalendarItemResponse FromEntity(ContentItem item)
    {
        if (!item.ScheduledDate.HasValue)
        {
            throw new InvalidOperationException(
                $"Cannot create CalendarItemResponse from ContentItem {item.Id} without a scheduled date.");
        }

        return new()
        {
            Id = item.Id,
            Title = item.Title,
            Status = item.Status,
            ScheduledDate = item.ScheduledDate.Value,
            PlatformTags = item.PlatformTags.ToList()
        };
    }
}

/// <summary>
/// Response DTO for calendar view grouped by date.
/// Story: ACF-006
/// </summary>
public sealed class CalendarResponse
{
    public int Year { get; init; }
    public int Month { get; init; }
    public IReadOnlyList<CalendarDayResponse> Days { get; init; } = Array.Empty<CalendarDayResponse>();
    public int TotalItems { get; init; }
}

/// <summary>
/// Response DTO for a single day in the calendar.
/// Story: ACF-006
/// </summary>
public sealed class CalendarDayResponse
{
    public DateOnly Date { get; init; }
    public IReadOnlyList<CalendarItemResponse> Items { get; init; } = Array.Empty<CalendarItemResponse>();
}
