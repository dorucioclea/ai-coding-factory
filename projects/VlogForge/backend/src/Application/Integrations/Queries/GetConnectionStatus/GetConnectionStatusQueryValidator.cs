using FluentValidation;

namespace VlogForge.Application.Integrations.Queries.GetConnectionStatus;

/// <summary>
/// Validator for GetConnectionStatusQuery.
/// Story: ACF-003
/// </summary>
public sealed class GetConnectionStatusQueryValidator : AbstractValidator<GetConnectionStatusQuery>
{
    public GetConnectionStatusQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
