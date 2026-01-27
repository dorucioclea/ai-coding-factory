using FluentValidation;

namespace VlogForge.Application.Analytics.Queries.GetOverview;

/// <summary>
/// Validator for GetAnalyticsOverviewQuery.
/// Story: ACF-004
/// </summary>
public sealed class GetAnalyticsOverviewQueryValidator : AbstractValidator<GetAnalyticsOverviewQuery>
{
    public GetAnalyticsOverviewQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
