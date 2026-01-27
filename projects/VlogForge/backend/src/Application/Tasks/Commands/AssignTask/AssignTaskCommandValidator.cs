using FluentValidation;

namespace VlogForge.Application.Tasks.Commands.AssignTask;

/// <summary>
/// Validator for AssignTaskCommand.
/// Story: ACF-008
/// </summary>
public sealed class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskCommandValidator()
    {
        RuleFor(x => x.ContentItemId)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.AssigneeId)
            .NotEmpty()
            .WithMessage("Assignee ID is required.");

        RuleFor(x => x.AssignedById)
            .NotEmpty()
            .WithMessage("Assigned by ID is required.");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("Due date is required.")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters.");
    }
}
