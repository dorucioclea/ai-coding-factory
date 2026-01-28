using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Collaborations.DTOs;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Collaborations.Queries.GetSentCollaborationRequests;

/// <summary>
/// Handler for getting sent collaboration requests.
/// Story: ACF-011
/// </summary>
public sealed partial class GetSentCollaborationRequestsQueryHandler
    : IRequestHandler<GetSentCollaborationRequestsQuery, CollaborationRequestListResponse>
{
    private readonly ICollaborationRequestRepository _collaborationRepo;
    private readonly ICreatorProfileRepository _profileRepo;
    private readonly ILogger<GetSentCollaborationRequestsQueryHandler> _logger;

    public GetSentCollaborationRequestsQueryHandler(
        ICollaborationRequestRepository collaborationRepo,
        ICreatorProfileRepository profileRepo,
        ILogger<GetSentCollaborationRequestsQueryHandler> logger)
    {
        _collaborationRepo = collaborationRepo;
        _profileRepo = profileRepo;
        _logger = logger;
    }

    public async Task<CollaborationRequestListResponse> Handle(
        GetSentCollaborationRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var (requests, totalCount) = await _collaborationRepo.GetSentRequestsAsync(
            request.UserId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Batch-load all profiles to avoid N+1 queries
        var userIds = requests
            .SelectMany(r => new[] { r.SenderId, r.RecipientId })
            .Distinct()
            .ToList();

        var profiles = new Dictionary<Guid, Domain.Entities.CreatorProfile>();
        foreach (var uid in userIds)
        {
            var profile = await _profileRepo.GetByUserIdAsync(uid, cancellationToken);
            if (profile is not null)
                profiles[uid] = profile;
        }

        var items = requests.Select(r =>
            CollaborationRequestResponse.FromEntity(
                r,
                profiles.GetValueOrDefault(r.SenderId),
                profiles.GetValueOrDefault(r.RecipientId)))
            .ToList();

        LogSentRetrieved(_logger, request.UserId, items.Count, totalCount);

        return new CollaborationRequestListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Retrieved {Count} sent requests for user {UserId} (total: {Total})")]
    private static partial void LogSentRetrieved(
        ILogger logger, Guid userId, int count, int total);
}
