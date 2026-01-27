using FluentValidation;

namespace VlogForge.Application.Tasks.Commands.AddTaskComment;

/// <summary>
/// Validator for AddTaskCommentCommand.
/// Story: ACF-008
/// </summary>
public sealed class AddTaskCommentCommandValidator : AbstractValidator<AddTaskCommentCommand>
{
    public AddTaskCommentCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required.");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("Author ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required.")
            .MaximumLength(2000)
            .WithMessage("Comment content cannot exceed 2000 characters.");
    }
}
