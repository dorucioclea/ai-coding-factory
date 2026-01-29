using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.SharedProjects.Commands.UpdateProjectTask;

/// <summary>
/// Validator for UpdateProjectTaskCommand.
/// Story: ACF-013
/// </summary>
public sealed class UpdateProjectTaskCommandValidator : AbstractValidator<UpdateProjectTaskCommand>
{
    public UpdateProjectTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title)
            .MaximumLength(SharedProjectTask.MaxTitleLength)
            .When(x => x.Title is not null);
        RuleFor(x => x.Description)
            .MaximumLength(SharedProjectTask.MaxDescriptionLength)
            .When(x => x.Description is not null);
    }
}
