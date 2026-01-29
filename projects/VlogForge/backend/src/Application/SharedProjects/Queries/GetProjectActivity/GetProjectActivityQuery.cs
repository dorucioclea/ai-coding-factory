using MediatR;
using VlogForge.Application.SharedProjects.DTOs;

namespace VlogForge.Application.SharedProjects.Queries.GetProjectActivity;

/// <summary>
/// Query to get activity feed for a shared project.
/// Story: ACF-013
/// </summary>
public sealed record GetProjectActivityQuery(
    Guid ProjectId,
    Guid UserId,
    int Page = 1,
    int PageSize = 50) : IRequest<SharedProjectActivityListResponse>;
