using FluentValidation;

namespace VlogForge.Application.Approvals.Queries.GetPendingApprovals;

/// <summary>
/// Validator for GetPendingApprovalsQuery.
/// Story: ACF-009
/// </summary>
public sealed class GetPendingApprovalsQueryValidator : AbstractValidator<GetPendingApprovalsQuery>
{
    public GetPendingApprovalsQueryValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
