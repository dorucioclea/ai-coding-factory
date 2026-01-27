using FluentValidation;

namespace VlogForge.Application.ContentIdeas.Commands.DeleteContentIdea;

/// <summary>
/// Validator for DeleteContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed class DeleteContentIdeaCommandValidator : AbstractValidator<DeleteContentIdeaCommand>
{
    public DeleteContentIdeaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
