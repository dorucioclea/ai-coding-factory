using FluentValidation;

namespace VlogForge.Application.Approvals.Queries.GetApprovalHistory;

/// <summary>
/// Validator for GetApprovalHistoryQuery.
/// Story: ACF-009
/// </summary>
public sealed class GetApprovalHistoryQueryValidator : AbstractValidator<GetApprovalHistoryQuery>
{
    public GetApprovalHistoryQueryValidator()
    {
        RuleFor(x => x.ContentItemId)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
