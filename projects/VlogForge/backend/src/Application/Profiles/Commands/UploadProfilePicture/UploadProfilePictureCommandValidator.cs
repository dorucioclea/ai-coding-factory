using FluentValidation;

namespace VlogForge.Application.Profiles.Commands.UploadProfilePicture;

/// <summary>
/// Validator for UploadProfilePictureCommand.
/// Story: ACF-002
/// </summary>
public sealed class UploadProfilePictureCommandValidator : AbstractValidator<UploadProfilePictureCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    ];

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    ];

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadProfilePictureCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ImageStream)
            .NotNull().WithMessage("Image stream is required.");

        When(x => x.ImageStream != null, () =>
        {
            RuleFor(x => x.ImageStream)
                .Must(stream => stream!.CanRead)
                    .WithMessage("Image stream must be readable.")
                .Must(stream => stream!.Length <= MaxFileSizeBytes)
                    .WithMessage($"Image file size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");
        });

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(HaveValidExtension)
                .WithMessage($"Invalid file extension. Allowed extensions: {string.Join(", ", AllowedExtensions)}");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(BeValidContentType)
                .WithMessage($"Invalid content type. Allowed types: {string.Join(", ", AllowedContentTypes)}");
    }

    private static bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private static bool BeValidContentType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
}
