using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Tasks.Commands.UpdateTaskStatus;

/// <summary>
/// Validator for UpdateTaskStatusCommand.
/// Story: ACF-008
/// </summary>
public sealed class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid status value.");

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty()
            .WithMessage("Updated by user ID is required.");
    }
}
