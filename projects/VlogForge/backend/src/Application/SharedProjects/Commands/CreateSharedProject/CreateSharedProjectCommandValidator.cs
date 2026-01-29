using FluentValidation;

namespace VlogForge.Application.SharedProjects.Commands.CreateSharedProject;

/// <summary>
/// Validator for CreateSharedProjectCommand.
/// Story: ACF-013
/// </summary>
public sealed class CreateSharedProjectCommandValidator : AbstractValidator<CreateSharedProjectCommand>
{
    public CreateSharedProjectCommandValidator()
    {
        RuleFor(x => x.CollaborationRequestId)
            .NotEmpty().WithMessage("Collaboration request ID is required.");

        RuleFor(x => x.SenderId)
            .NotEmpty().WithMessage("Sender ID is required.");

        RuleFor(x => x.RecipientId)
            .NotEmpty().WithMessage("Recipient ID is required.")
            .NotEqual(x => x.SenderId).WithMessage("Sender and recipient cannot be the same.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(Domain.Entities.SharedProject.MaxNameLength)
            .WithMessage($"Project name cannot exceed {Domain.Entities.SharedProject.MaxNameLength} characters.");
    }
}
