using FluentValidation;

namespace VlogForge.Application.Analytics.Queries.GetTopContent;

/// <summary>
/// Validator for GetTopContentQuery.
/// Story: ACF-004
/// </summary>
public sealed class GetTopContentQueryValidator : AbstractValidator<GetTopContentQuery>
{
    private static readonly string[] AllowedSortOptions = ["views", "engagement", "likes", "comments"];

    public GetTopContentQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");

        RuleFor(x => x.SortBy)
            .NotEmpty().WithMessage("Sort option is required.")
            .Must(sortBy => AllowedSortOptions.Contains(sortBy.ToLowerInvariant()))
            .WithMessage($"Sort option must be one of: {string.Join(", ", AllowedSortOptions)}");
    }
}
