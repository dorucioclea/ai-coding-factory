using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for SharedProject aggregate operations.
/// Story: ACF-013
/// </summary>
public interface ISharedProjectRepository
{
    /// <summary>
    /// Gets a shared project by ID with all related data.
    /// </summary>
    Task<SharedProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets shared projects for a user with pagination.
    /// </summary>
    Task<(IReadOnlyList<SharedProject> Projects, int TotalCount)> GetByMemberUserIdPagedAsync(
        Guid userId,
        SharedProjectStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a shared project by collaboration request ID.
    /// </summary>
    Task<SharedProject?> GetByCollaborationRequestIdAsync(
        Guid collaborationRequestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated activity feed for a project.
    /// </summary>
    Task<(IReadOnlyList<SharedProjectActivity> Activities, int TotalCount)> GetProjectActivityPagedAsync(
        Guid projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new shared project.
    /// </summary>
    Task AddAsync(SharedProject project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing shared project.
    /// </summary>
    Task UpdateAsync(SharedProject project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
