using VlogForge.Domain.Entities;

namespace VlogForge.Application.Collaborations.DTOs;

/// <summary>
/// DTO for a collaboration request.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestResponse
{
    public Guid Id { get; init; }
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public string SenderDisplayName { get; init; } = string.Empty;
    public string SenderUsername { get; init; } = string.Empty;
    public string? SenderProfilePictureUrl { get; init; }
    public string RecipientDisplayName { get; init; } = string.Empty;
    public string RecipientUsername { get; init; } = string.Empty;
    public string? RecipientProfilePictureUrl { get; init; }
    public string Message { get; init; } = string.Empty;
    public CollaborationRequestStatus Status { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? RespondedAt { get; init; }
    public string? DeclineReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsExpired { get; init; }

    public static CollaborationRequestResponse FromEntity(
        CollaborationRequest request,
        CreatorProfile? senderProfile = null,
        CreatorProfile? recipientProfile = null)
    {
        return new CollaborationRequestResponse
        {
            Id = request.Id,
            SenderId = request.SenderId,
            RecipientId = request.RecipientId,
            SenderDisplayName = senderProfile?.DisplayName ?? string.Empty,
            SenderUsername = senderProfile?.Username ?? string.Empty,
            SenderProfilePictureUrl = senderProfile?.ProfilePictureUrl,
            RecipientDisplayName = recipientProfile?.DisplayName ?? string.Empty,
            RecipientUsername = recipientProfile?.Username ?? string.Empty,
            RecipientProfilePictureUrl = recipientProfile?.ProfilePictureUrl,
            Message = request.Message,
            Status = request.Status,
            ExpiresAt = request.ExpiresAt,
            RespondedAt = request.RespondedAt,
            DeclineReason = request.DeclineReason,
            CreatedAt = request.CreatedAt,
            IsExpired = request.IsExpired
        };
    }
}

/// <summary>
/// Paginated list of collaboration requests.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestListResponse
{
    public IReadOnlyList<CollaborationRequestResponse> Items { get; init; } = new List<CollaborationRequestResponse>();
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
