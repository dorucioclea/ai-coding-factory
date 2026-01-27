using FluentValidation;

namespace VlogForge.Application.ContentIdeas.Queries.GetContentIdeas;

/// <summary>
/// Validator for GetContentIdeasQuery.
/// Story: ACF-005
/// </summary>
public sealed class GetContentIdeasQueryValidator : AbstractValidator<GetContentIdeasQuery>
{
    private const int MaxSearchTermLength = 200;
    private const int MaxPlatformTagLength = 50;

    public GetContentIdeasQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid status value.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(MaxSearchTermLength)
            .WithMessage($"Search term cannot exceed {MaxSearchTermLength} characters.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.PlatformTag)
            .MaximumLength(MaxPlatformTagLength)
            .WithMessage($"Platform tag cannot exceed {MaxPlatformTagLength} characters.")
            .When(x => !string.IsNullOrEmpty(x.PlatformTag));
    }
}
