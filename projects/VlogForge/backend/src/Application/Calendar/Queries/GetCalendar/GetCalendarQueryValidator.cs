using FluentValidation;

namespace VlogForge.Application.Calendar.Queries.GetCalendar;

/// <summary>
/// Validator for GetCalendarQuery.
/// Story: ACF-006
/// </summary>
public sealed class GetCalendarQueryValidator : AbstractValidator<GetCalendarQuery>
{
    public GetCalendarQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2100)
            .WithMessage("Year must be between 2020 and 2100.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12.");
    }
}
