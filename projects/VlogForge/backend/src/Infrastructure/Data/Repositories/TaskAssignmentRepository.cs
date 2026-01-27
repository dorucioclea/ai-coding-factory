using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for TaskAssignment aggregate.
/// Story: ACF-008
/// </summary>
public sealed class TaskAssignmentRepository : ITaskAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public TaskAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TaskAssignment?> GetByIdWithCommentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TaskAssignment?> GetByIdWithHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .Include(t => t.History.OrderByDescending(h => h.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskAssignment>> GetByAssigneeIdAsync(
        Guid assigneeId,
        bool includeCompleted = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TaskAssignments
            .Where(t => t.AssigneeId == assigneeId);

        if (!includeCompleted)
        {
            query = query.Where(t => t.Status != AssignmentStatus.Completed);
        }

        return await query
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<TaskAssignment> Tasks, int TotalCount)> GetByAssigneeIdPagedAsync(
        Guid assigneeId,
        int page,
        int pageSize,
        bool includeCompleted = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TaskAssignments
            .Where(t => t.AssigneeId == assigneeId);

        if (!includeCompleted)
        {
            query = query.Where(t => t.Status != AssignmentStatus.Completed);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (tasks, totalCount);
    }

    public async Task<IReadOnlyList<TaskAssignment>> GetByContentItemIdAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .Where(t => t.ContentItemId == contentItemId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskAssignment>> GetByTeamIdAsync(
        Guid teamId,
        bool includeCompleted = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TaskAssignments
            .Where(t => t.TeamId == teamId);

        if (!includeCompleted)
        {
            query = query.Where(t => t.Status != AssignmentStatus.Completed);
        }

        return await query
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveForContentItemAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .AnyAsync(t => t.ContentItemId == contentItemId &&
                          t.Status != AssignmentStatus.Completed,
                      cancellationToken);
    }

    public async Task<IReadOnlyList<TaskAssignment>> GetOverdueTasksAsync(
        Guid? teamId = null,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = _context.TaskAssignments
            .Where(t => t.Status != AssignmentStatus.Completed && t.DueDate < now);

        if (teamId.HasValue)
        {
            query = query.Where(t => t.TeamId == teamId.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        return await query
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskAssignment task, CancellationToken cancellationToken = default)
    {
        await _context.TaskAssignments.AddAsync(task, cancellationToken);
    }

    public Task UpdateAsync(TaskAssignment task, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(task);
        if (entry.State == EntityState.Detached)
        {
            _context.TaskAssignments.Update(task);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskAssignment task, CancellationToken cancellationToken = default)
    {
        _context.TaskAssignments.Remove(task);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
