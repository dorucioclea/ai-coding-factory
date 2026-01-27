using FluentValidation;

namespace VlogForge.Application.Discovery.Queries.DiscoverCreators;

/// <summary>
/// Validator for DiscoverCreatorsQuery.
/// Story: ACF-010
/// </summary>
public sealed class DiscoverCreatorsQueryValidator : AbstractValidator<DiscoverCreatorsQuery>
{
    public DiscoverCreatorsQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => x.SearchTerm is not null)
            .WithMessage("Search term cannot exceed 100 characters.");

        RuleFor(x => x.Niches)
            .Must(niches => niches is null || niches.Count <= 10)
            .WithMessage("Cannot filter by more than 10 niches at once.");

        RuleForEach(x => x.Niches)
            .MaximumLength(50)
            .When(x => x.Niches is not null)
            .WithMessage("Each niche tag cannot exceed 50 characters.");

        RuleFor(x => x.Platforms)
            .Must(platforms => platforms is null || platforms.Count <= 5)
            .WithMessage("Cannot filter by more than 5 platforms at once.");
    }
}
