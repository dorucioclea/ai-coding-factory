using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.DTOs;

namespace VlogForge.Application.Tasks.Queries.GetMyTasks;

/// <summary>
/// Handler for GetMyTasksQuery.
/// Stories: ACF-008, ACF-014
/// </summary>
public sealed partial class GetMyTasksQueryHandler : IRequestHandler<GetMyTasksQuery, TaskListResponse>
{
    private readonly ITaskAssignmentRepository _repository;
    private readonly ILogger<GetMyTasksQueryHandler> _logger;

    public GetMyTasksQueryHandler(
        ITaskAssignmentRepository repository,
        ILogger<GetMyTasksQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TaskListResponse> Handle(GetMyTasksQuery request, CancellationToken cancellationToken)
    {
        var (tasks, totalCount) = await _repository.GetByAssigneeIdFilteredPagedAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            request.Status,
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        LogTasksRetrieved(_logger, request.UserId, tasks.Count, totalCount);

        return new TaskListResponse
        {
            Items = tasks.Select(t => TaskAssignmentResponse.FromEntity(t)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieved {Count} tasks for user {UserId} (total: {Total})")]
    private static partial void LogTasksRetrieved(ILogger logger, Guid userId, int count, int total);
}
