using FluentValidation;

namespace VlogForge.Application.Analytics.Queries.GetTrends;

/// <summary>
/// Validator for GetAnalyticsTrendsQuery.
/// Story: ACF-004
/// </summary>
public sealed class GetAnalyticsTrendsQueryValidator : AbstractValidator<GetAnalyticsTrendsQuery>
{
    private static readonly string[] AllowedPeriods = ["7d", "30d", "90d"];

    public GetAnalyticsTrendsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Period)
            .NotEmpty().WithMessage("Period is required.")
            .Must(period => AllowedPeriods.Contains(period.ToLowerInvariant()))
            .WithMessage($"Period must be one of: {string.Join(", ", AllowedPeriods)}");
    }
}
