using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// Implementation of IDateTimeService.
/// </summary>
public class DateTimeService : IDateTimeService
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime Now => DateTime.Now;

    /// <inheritdoc/>
    public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
}
