using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Status workflow for content ideas.
/// Story: ACF-005, ACF-009
/// </summary>
public enum IdeaStatus
{
    /// <summary>Initial idea stage.</summary>
    Idea = 0,

    /// <summary>Being drafted/written.</summary>
    Draft = 1,

    /// <summary>Under review before scheduling.</summary>
    InReview = 2,

    /// <summary>Scheduled for publishing.</summary>
    Scheduled = 3,

    /// <summary>Published content.</summary>
    Published = 4,

    /// <summary>Approved by approver, ready to be scheduled. Story: ACF-009</summary>
    Approved = 5,

    /// <summary>Approver requested changes, needs revision. Story: ACF-009</summary>
    ChangesRequested = 6
}

/// <summary>
/// Entity representing a content idea in the creator's pipeline.
/// Supports soft delete with 30-day recovery window.
/// Story: ACF-005
/// </summary>
public sealed class ContentItem : AggregateRoot
{
    public const int MaxTitleLength = 200;
    public const int MaxNotesLength = 5000;
    public const int MaxPlatformTags = 10;

    private readonly List<string> _platformTags = new();

    /// <summary>
    /// Gets the user ID this content item belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the title of the content idea.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the notes/description for the content idea.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets the current status in the workflow.
    /// </summary>
    public IdeaStatus Status { get; private set; }

    /// <summary>
    /// Gets the platform tags (e.g., YouTube, TikTok, Instagram).
    /// </summary>
    public IReadOnlyCollection<string> PlatformTags => _platformTags.AsReadOnly();

    /// <summary>
    /// Gets whether the item has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Gets when the item was soft-deleted (for 30-day cleanup).
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Gets the scheduled publication date for the content.
    /// Story: ACF-006
    /// </summary>
    public DateTime? ScheduledDate { get; private set; }

    private ContentItem() : base()
    {
        Title = string.Empty;
    }

    private ContentItem(Guid userId, string title, string? notes) : base()
    {
        UserId = userId;
        Title = title;
        Notes = notes;
        Status = IdeaStatus.Idea;
        IsDeleted = false;
    }

    /// <summary>
    /// Creates a new content item.
    /// </summary>
    /// <param name="userId">The owner's user ID.</param>
    /// <param name="title">The content title.</param>
    /// <param name="notes">Optional notes/description.</param>
    /// <returns>A new ContentItem instance.</returns>
    public static ContentItem Create(Guid userId, string title, string? notes)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ValidateTitle(title);
        ValidateNotes(notes);

        var item = new ContentItem(userId, title.Trim(), notes?.Trim());
        item.RaiseDomainEvent(new ContentItemCreatedEvent(item.Id, item.UserId, item.Title));

        return item;
    }

    /// <summary>
    /// Updates the content item's title and notes.
    /// </summary>
    /// <param name="title">The new title.</param>
    /// <param name="notes">The new notes.</param>
    public void Update(string title, string? notes)
    {
        EnsureNotDeleted();
        ValidateTitle(title);
        ValidateNotes(notes);

        Title = title.Trim();
        Notes = notes?.Trim();
        IncrementVersion();

        RaiseDomainEvent(new ContentItemUpdatedEvent(Id, UserId));
    }

    /// <summary>
    /// Updates the status in the workflow.
    /// </summary>
    /// <param name="newStatus">The target status.</param>
    public void UpdateStatus(IdeaStatus newStatus)
    {
        EnsureNotDeleted();

        if (Status == newStatus)
            return;

        ValidateStatusTransition(Status, newStatus);

        var oldStatus = Status;
        Status = newStatus;
        IncrementVersion();

        RaiseDomainEvent(new ContentItemStatusChangedEvent(Id, UserId, oldStatus, newStatus));
    }

    /// <summary>
    /// Adds a platform tag.
    /// </summary>
    /// <param name="tag">The platform tag to add.</param>
    /// <returns>True if added, false if already exists or limit reached.</returns>
    public bool AddPlatformTag(string tag)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(tag))
            return false;

        if (_platformTags.Count >= MaxPlatformTags)
            return false;

        var normalized = tag.Trim().ToLowerInvariant();

        if (_platformTags.Contains(normalized))
            return false;

        _platformTags.Add(normalized);
        IncrementVersion();
        return true;
    }

    /// <summary>
    /// Removes a platform tag.
    /// </summary>
    /// <param name="tag">The platform tag to remove.</param>
    /// <returns>True if removed, false if not found.</returns>
    public bool RemovePlatformTag(string tag)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(tag))
            return false;

        var normalized = tag.Trim().ToLowerInvariant();
        var removed = _platformTags.Remove(normalized);

        if (removed)
        {
            IncrementVersion();
        }

        return removed;
    }

    /// <summary>
    /// Sets all platform tags, replacing existing ones.
    /// </summary>
    /// <param name="tags">The platform tags to set.</param>
    public void SetPlatformTags(IEnumerable<string> tags)
    {
        EnsureNotDeleted();

        var normalizedTags = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .Take(MaxPlatformTags)
            .ToList();

        _platformTags.Clear();
        _platformTags.AddRange(normalizedTags);
        IncrementVersion();
    }

    /// <summary>
    /// Soft-deletes the content item.
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        IncrementVersion();

        RaiseDomainEvent(new ContentItemDeletedEvent(Id, UserId));
    }

    /// <summary>
    /// Restores a soft-deleted content item.
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        DeletedAt = null;
        IncrementVersion();

        RaiseDomainEvent(new ContentItemRestoredEvent(Id, UserId));
    }

    /// <summary>
    /// Updates the scheduled publication date.
    /// Story: ACF-006
    /// </summary>
    /// <param name="scheduledDate">The new scheduled date.</param>
    public void UpdateScheduledDate(DateTime scheduledDate)
    {
        EnsureNotDeleted();

        // Normalize to UTC if not already
        var normalizedDate = scheduledDate.Kind == DateTimeKind.Utc
            ? scheduledDate
            : DateTime.SpecifyKind(scheduledDate, DateTimeKind.Utc);

        // Check if date is actually changing (with tolerance for precision)
        if (ScheduledDate.HasValue &&
            Math.Abs((ScheduledDate.Value - normalizedDate).TotalSeconds) < 1)
        {
            return;
        }

        var oldDate = ScheduledDate;
        ScheduledDate = normalizedDate;
        IncrementVersion();

        RaiseDomainEvent(new ContentItemScheduledDateChangedEvent(Id, UserId, oldDate, ScheduledDate));
    }

    /// <summary>
    /// Clears the scheduled publication date.
    /// Story: ACF-006
    /// </summary>
    public void ClearScheduledDate()
    {
        EnsureNotDeleted();

        if (!ScheduledDate.HasValue)
            return;

        var oldDate = ScheduledDate;
        ScheduledDate = null;
        IncrementVersion();

        RaiseDomainEvent(new ContentItemScheduledDateChangedEvent(Id, UserId, oldDate, null));
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted content item.");
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (title.Trim().Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.", nameof(title));
    }

    private static void ValidateNotes(string? notes)
    {
        if (notes != null && notes.Length > MaxNotesLength)
            throw new ArgumentException($"Notes cannot exceed {MaxNotesLength} characters.", nameof(notes));
    }

    private static void ValidateStatusTransition(IdeaStatus from, IdeaStatus to)
    {
        // Define valid transitions for the approval workflow (ACF-009)
        // Workflow: Idea → Draft → InReview → Approved → Scheduled → Published
        //                    ↑       ↓                ↓
        //                    └─ ChangesRequested ←────┘
        //                          ↓
        //                   (can go to Draft or directly to InReview)
        var validTransitions = new Dictionary<IdeaStatus, IdeaStatus[]>
        {
            { IdeaStatus.Idea, new[] { IdeaStatus.Draft } },
            { IdeaStatus.Draft, new[] { IdeaStatus.Idea, IdeaStatus.InReview } },
            { IdeaStatus.InReview, new[] { IdeaStatus.Draft, IdeaStatus.Approved, IdeaStatus.ChangesRequested } },
            { IdeaStatus.Approved, new[] { IdeaStatus.InReview, IdeaStatus.Scheduled } },
            { IdeaStatus.ChangesRequested, new[] { IdeaStatus.Draft, IdeaStatus.InReview } },
            { IdeaStatus.Scheduled, new[] { IdeaStatus.Approved, IdeaStatus.Published } },
            { IdeaStatus.Published, new[] { IdeaStatus.Scheduled } }
        };

        if (!validTransitions.TryGetValue(from, out var allowedTargets) ||
            !allowedTargets.Contains(to))
        {
            throw new InvalidOperationException($"Cannot transition from {from} to {to}. Invalid status transition.");
        }
    }

    /// <summary>
    /// Gets the valid status transitions from the current status.
    /// Story: ACF-009
    /// </summary>
    /// <returns>Array of valid target statuses.</returns>
    public IdeaStatus[] GetValidTransitions()
    {
        return Status switch
        {
            IdeaStatus.Idea => new[] { IdeaStatus.Draft },
            IdeaStatus.Draft => new[] { IdeaStatus.Idea, IdeaStatus.InReview },
            IdeaStatus.InReview => new[] { IdeaStatus.Draft, IdeaStatus.Approved, IdeaStatus.ChangesRequested },
            IdeaStatus.Approved => new[] { IdeaStatus.InReview, IdeaStatus.Scheduled },
            IdeaStatus.ChangesRequested => new[] { IdeaStatus.Draft, IdeaStatus.InReview },
            IdeaStatus.Scheduled => new[] { IdeaStatus.Approved, IdeaStatus.Published },
            IdeaStatus.Published => new[] { IdeaStatus.Scheduled },
            _ => Array.Empty<IdeaStatus>()
        };
    }
}
