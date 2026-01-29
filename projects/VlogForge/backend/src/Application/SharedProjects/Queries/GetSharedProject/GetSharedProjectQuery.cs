using MediatR;
using VlogForge.Application.SharedProjects.DTOs;

namespace VlogForge.Application.SharedProjects.Queries.GetSharedProject;

/// <summary>
/// Query to get a shared project by ID.
/// Story: ACF-013
/// </summary>
public sealed record GetSharedProjectQuery(Guid ProjectId, Guid UserId)
    : IRequest<SharedProjectDetailResponse?>;
