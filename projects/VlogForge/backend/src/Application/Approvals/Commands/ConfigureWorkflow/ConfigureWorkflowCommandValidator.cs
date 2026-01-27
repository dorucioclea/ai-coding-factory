using FluentValidation;

namespace VlogForge.Application.Approvals.Commands.ConfigureWorkflow;

/// <summary>
/// Validator for ConfigureWorkflowCommand.
/// Story: ACF-009
/// </summary>
public sealed class ConfigureWorkflowCommandValidator : AbstractValidator<ConfigureWorkflowCommand>
{
    public ConfigureWorkflowCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
