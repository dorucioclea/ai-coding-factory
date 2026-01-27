using FluentValidation;

namespace VlogForge.Application.ContentIdeas.Commands.UpdateContentIdeaStatus;

/// <summary>
/// Validator for UpdateContentIdeaStatusCommand.
/// Story: ACF-005
/// </summary>
public sealed class UpdateContentIdeaStatusCommandValidator : AbstractValidator<UpdateContentIdeaStatusCommand>
{
    public UpdateContentIdeaStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid status value.");
    }
}
