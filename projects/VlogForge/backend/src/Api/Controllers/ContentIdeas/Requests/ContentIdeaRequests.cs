using VlogForge.Domain.Entities;

namespace VlogForge.Api.Controllers.ContentIdeas.Requests;

/// <summary>
/// Request to create a new content idea.
/// Story: ACF-005
/// </summary>
public sealed class CreateContentIdeaRequest
{
    /// <summary>
    /// The title of the content idea.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Optional notes/description.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Platform tags (e.g., YouTube, TikTok).
    /// </summary>
    public List<string>? PlatformTags { get; init; }
}

/// <summary>
/// Request to update a content idea.
/// Story: ACF-005
/// </summary>
public sealed class UpdateContentIdeaRequest
{
    /// <summary>
    /// The updated title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The updated notes.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// The updated platform tags.
    /// </summary>
    public List<string>? PlatformTags { get; init; }

    /// <summary>
    /// The new status (optional).
    /// </summary>
    public IdeaStatus? Status { get; init; }
}

/// <summary>
/// Request to update a content idea's status.
/// Story: ACF-005
/// </summary>
public sealed class UpdateStatusRequest
{
    /// <summary>
    /// The new status.
    /// </summary>
    public IdeaStatus Status { get; init; }
}
