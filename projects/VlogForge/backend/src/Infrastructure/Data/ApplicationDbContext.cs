using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VlogForge.Domain.Common;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Interfaces;

namespace VlogForge.Infrastructure.Data;

/// <summary>
/// Application database context implementing Unit of Work pattern.
/// </summary>
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Users DbSet.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets or sets the RefreshTokens DbSet.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Gets or sets the CreatorProfiles DbSet.
    /// Story: ACF-002
    /// </summary>
    public DbSet<CreatorProfile> CreatorProfiles => Set<CreatorProfile>();

    /// <summary>
    /// Gets or sets the ConnectedPlatforms DbSet.
    /// Story: ACF-002
    /// </summary>
    public DbSet<ConnectedPlatform> ConnectedPlatforms => Set<ConnectedPlatform>();

    /// <summary>
    /// Gets or sets the PlatformConnections DbSet (OAuth integration).
    /// Story: ACF-003
    /// </summary>
    public DbSet<PlatformConnection> PlatformConnections => Set<PlatformConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // CreatedAt is set in Entity constructor
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdatedBy("system"); // Replace with actual user
                    break;
            }
        }

        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await (_currentTransaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await (_currentTransaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask);
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Dispatches domain events from all tracked entities.
    /// </summary>
    private Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        // Dispatch events using MediatR (inject IMediator if needed)
        // foreach (var domainEvent in domainEvents)
        // {
        //     await _mediator.Publish(domainEvent, cancellationToken);
        // }
        return Task.CompletedTask;
    }
}
