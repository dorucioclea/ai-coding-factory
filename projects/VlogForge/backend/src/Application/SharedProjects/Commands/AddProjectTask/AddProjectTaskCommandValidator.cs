using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.SharedProjects.Commands.AddProjectTask;

/// <summary>
/// Validator for AddProjectTaskCommand.
/// Story: ACF-013
/// </summary>
public sealed class AddProjectTaskCommandValidator : AbstractValidator<AddProjectTaskCommand>
{
    public AddProjectTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(SharedProjectTask.MaxTitleLength);
        RuleFor(x => x.Description)
            .MaximumLength(SharedProjectTask.MaxDescriptionLength)
            .When(x => x.Description is not null);
    }
}
