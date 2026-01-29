using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for SharedProject.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectRepository : ISharedProjectRepository
{
    private readonly ApplicationDbContext _context;

    public SharedProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SharedProject?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SharedProjects
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Include(p => p.Links)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<SharedProject> Projects, int TotalCount)> GetByMemberUserIdPagedAsync(
        Guid userId,
        SharedProjectStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SharedProjects
            .AsNoTracking()
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Include(p => p.Links)
            .Where(p => p.Members.Any(m => m.UserId == userId));

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (projects, totalCount);
    }

    public async Task<SharedProject?> GetByCollaborationRequestIdAsync(
        Guid collaborationRequestId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SharedProjects
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Include(p => p.Links)
            .FirstOrDefaultAsync(p => p.CollaborationRequestId == collaborationRequestId, cancellationToken);
    }

    public async Task<(IReadOnlyList<SharedProjectActivity> Activities, int TotalCount)> GetProjectActivityPagedAsync(
        Guid projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SharedProjectActivities
            .AsNoTracking()
            .Where(a => a.SharedProjectId == projectId);

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (activities, totalCount);
    }

    public async Task AddAsync(
        SharedProject project,
        CancellationToken cancellationToken = default)
    {
        await _context.SharedProjects.AddAsync(project, cancellationToken);
    }

    public Task UpdateAsync(
        SharedProject project,
        CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(project);
        if (entry.State == EntityState.Detached)
            _context.SharedProjects.Update(project);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
