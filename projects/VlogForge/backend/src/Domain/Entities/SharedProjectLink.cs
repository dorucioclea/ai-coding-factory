using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Represents a link/resource shared in a project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectLink : Entity
{
    public const int MaxTitleLength = 200;
    public const int MaxUrlLength = 2000;
    public const int MaxDescriptionLength = 500;

    /// <summary>
    /// Gets the shared project ID.
    /// </summary>
    public Guid SharedProjectId { get; private set; }

    /// <summary>
    /// Gets the user ID who added this link.
    /// </summary>
    public Guid AddedByUserId { get; private set; }

    /// <summary>
    /// Gets the link title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the link URL.
    /// </summary>
    public string Url { get; private set; }

    /// <summary>
    /// Gets the optional link description.
    /// </summary>
    public string? Description { get; private set; }

    private SharedProjectLink() : base()
    {
        Title = string.Empty;
        Url = string.Empty;
    }

    private SharedProjectLink(Guid sharedProjectId, Guid addedByUserId,
        string title, string url, string? description) : base()
    {
        SharedProjectId = sharedProjectId;
        AddedByUserId = addedByUserId;
        Title = title;
        Url = url;
        Description = description;
    }

    /// <summary>
    /// Creates a new shared project link.
    /// </summary>
    public static SharedProjectLink Create(Guid sharedProjectId, Guid addedByUserId,
        string title, string url, string? description = null)
    {
        ValidateTitle(title);
        ValidateUrl(url);
        ValidateDescription(description);

        return new SharedProjectLink(sharedProjectId, addedByUserId,
            title.Trim(), url.Trim(), description?.Trim());
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Link title cannot be empty.", nameof(title));

        if (title.Trim().Length > MaxTitleLength)
            throw new ArgumentException($"Link title cannot exceed {MaxTitleLength} characters.", nameof(title));
    }

    private static void ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        if (url.Trim().Length > MaxUrlLength)
            throw new ArgumentException($"URL cannot exceed {MaxUrlLength} characters.", nameof(url));

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
            throw new ArgumentException("URL must be a valid HTTP or HTTPS URL.", nameof(url));
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Link description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
    }
}
