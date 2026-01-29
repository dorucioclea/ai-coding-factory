using MediatR;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.SharedProjects.Queries.GetUserSharedProjects;

/// <summary>
/// Query to get shared projects for a user.
/// Story: ACF-013
/// </summary>
public sealed record GetUserSharedProjectsQuery(
    Guid UserId,
    SharedProjectStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<SharedProjectListResponse>;
