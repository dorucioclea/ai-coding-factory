using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Team aggregate.
/// Story: ACF-007
/// </summary>
public sealed class TeamRepository : ITeamRepository
{
    private readonly ApplicationDbContext _context;

    public TeamRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Team?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Team?> GetByIdWithInvitationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Include(t => t.Invitations)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Team>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.OwnerId == ownerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Team>> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.Members.Any(m => m.UserId == userId))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Team> Teams, int TotalCount)> GetByMemberUserIdPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Teams
            .Include(t => t.Members)
            .Where(t => t.Members.Any(m => m.UserId == userId));

        var totalCount = await query.CountAsync(cancellationToken);

        var teams = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (teams, totalCount);
    }

    public async Task<Team?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Include(t => t.Invitations)
            .FirstOrDefaultAsync(t => t.Invitations.Any(i => i.Token == token && i.AcceptedAt == null), cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(Guid ownerId, string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        return await _context.Teams
            .AnyAsync(t => t.OwnerId == ownerId &&
                          EF.Functions.ILike(t.Name, normalizedName),
                      cancellationToken);
    }

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        await _context.Teams.AddAsync(team, cancellationToken);
    }

    public Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(team);
        if (entry.State == EntityState.Detached)
        {
            _context.Teams.Update(team);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Team team, CancellationToken cancellationToken = default)
    {
        _context.Teams.Remove(team);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AreInSameTeamAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .AnyAsync(t =>
                t.Members.Any(m => m.UserId == userId1) &&
                t.Members.Any(m => m.UserId == userId2),
                cancellationToken);
    }
}
