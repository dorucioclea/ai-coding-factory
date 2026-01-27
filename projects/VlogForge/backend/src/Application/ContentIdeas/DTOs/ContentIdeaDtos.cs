using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.DTOs;

/// <summary>
/// Response DTO for a content idea.
/// Story: ACF-005
/// </summary>
public sealed class ContentIdeaResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public IdeaStatus Status { get; init; }
    public IReadOnlyList<string> PlatformTags { get; init; } = Array.Empty<string>();
    public DateTime? ScheduledDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Creates a response from a ContentItem entity.
    /// </summary>
    public static ContentIdeaResponse FromEntity(ContentItem item) => new()
    {
        Id = item.Id,
        UserId = item.UserId,
        Title = item.Title,
        Notes = item.Notes,
        Status = item.Status,
        PlatformTags = item.PlatformTags.ToList(),
        ScheduledDate = item.ScheduledDate,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}

/// <summary>
/// Response DTO for a list of content ideas.
/// Story: ACF-005
/// </summary>
public sealed class ContentIdeasListResponse
{
    public IReadOnlyList<ContentIdeaResponse> Items { get; init; } = Array.Empty<ContentIdeaResponse>();
    public int TotalCount { get; init; }
}
