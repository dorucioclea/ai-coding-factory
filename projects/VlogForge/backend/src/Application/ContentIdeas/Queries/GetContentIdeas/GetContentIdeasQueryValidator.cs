using FluentValidation;

namespace VlogForge.Application.ContentIdeas.Queries.GetContentIdeas;

/// <summary>
/// Validator for GetContentIdeasQuery.
/// Story: ACF-005
/// </summary>
public sealed class GetContentIdeasQueryValidator : AbstractValidator<GetContentIdeasQuery>
{
    public GetContentIdeasQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid status value.");
    }
}
