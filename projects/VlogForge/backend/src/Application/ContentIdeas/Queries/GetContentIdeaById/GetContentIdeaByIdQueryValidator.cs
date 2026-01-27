using FluentValidation;

namespace VlogForge.Application.ContentIdeas.Queries.GetContentIdeaById;

/// <summary>
/// Validator for GetContentIdeaByIdQuery.
/// Story: ACF-005
/// </summary>
public sealed class GetContentIdeaByIdQueryValidator : AbstractValidator<GetContentIdeaByIdQuery>
{
    public GetContentIdeaByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
