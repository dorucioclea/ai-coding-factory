using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Collaborations.DTOs;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Collaborations.Queries.GetCollaborationInbox;

/// <summary>
/// Handler for getting collaboration inbox.
/// Story: ACF-011
/// </summary>
public sealed partial class GetCollaborationInboxQueryHandler
    : IRequestHandler<GetCollaborationInboxQuery, CollaborationRequestListResponse>
{
    private readonly ICollaborationRequestRepository _collaborationRepo;
    private readonly ICreatorProfileRepository _profileRepo;
    private readonly ILogger<GetCollaborationInboxQueryHandler> _logger;

    public GetCollaborationInboxQueryHandler(
        ICollaborationRequestRepository collaborationRepo,
        ICreatorProfileRepository profileRepo,
        ILogger<GetCollaborationInboxQueryHandler> logger)
    {
        _collaborationRepo = collaborationRepo;
        _profileRepo = profileRepo;
        _logger = logger;
    }

    public async Task<CollaborationRequestListResponse> Handle(
        GetCollaborationInboxQuery request,
        CancellationToken cancellationToken)
    {
        var (requests, totalCount) = await _collaborationRepo.GetReceivedRequestsAsync(
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

        LogInboxRetrieved(_logger, request.UserId, items.Count, totalCount);

        return new CollaborationRequestListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Retrieved {Count} inbox requests for user {UserId} (total: {Total})")]
    private static partial void LogInboxRetrieved(
        ILogger logger, Guid userId, int count, int total);
}
