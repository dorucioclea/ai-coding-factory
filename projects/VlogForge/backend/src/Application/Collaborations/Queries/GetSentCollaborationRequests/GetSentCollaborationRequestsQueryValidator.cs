using FluentValidation;

namespace VlogForge.Application.Collaborations.Queries.GetSentCollaborationRequests;

/// <summary>
/// Validator for GetSentCollaborationRequestsQuery.
/// Story: ACF-011
/// </summary>
public sealed class GetSentCollaborationRequestsQueryValidator
    : AbstractValidator<GetSentCollaborationRequestsQuery>
{
    public GetSentCollaborationRequestsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
