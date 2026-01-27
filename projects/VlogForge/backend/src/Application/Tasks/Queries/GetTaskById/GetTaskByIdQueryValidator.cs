using FluentValidation;

namespace VlogForge.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Validator for GetTaskByIdQuery.
/// Story: ACF-008
/// </summary>
public sealed class GetTaskByIdQueryValidator : AbstractValidator<GetTaskByIdQuery>
{
    public GetTaskByIdQueryValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("Requesting user ID is required.");
    }
}
