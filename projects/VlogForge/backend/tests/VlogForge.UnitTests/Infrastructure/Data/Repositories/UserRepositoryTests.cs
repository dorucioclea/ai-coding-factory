using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using VlogForge.Infrastructure.Data;
using VlogForge.Infrastructure.Data.Repositories;
using Xunit;

namespace VlogForge.UnitTests.Infrastructure.Data.Repositories;

/// <summary>
/// Unit tests for UserRepository, specifically testing the EF Core entity state
/// manipulation for RefreshToken tracking.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task UpdateAsyncWithNewRefreshTokenShouldMarkTokenAsAdded()
    {
        // Arrange - Create and save a user with an initial token
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashedPassword123");
        user.AddRefreshToken("initial-token-hash", DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Clear the change tracker to simulate a new request
        _context.ChangeTracker.Clear();

        // Load the user with tokens (simulating GetByRefreshTokenHashAsync)
        var loadedUser = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstAsync(u => u.Id == user.Id);

        // Act - Add a new refresh token (simulating refresh token rotation)
        var existingToken = loadedUser.RefreshTokens.First();
        existingToken.Revoke("test", "new-token-hash");
        loadedUser.AddRefreshToken("new-token-hash", DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");

        await _repository.UpdateAsync(loadedUser);

        // Assert - Check entity states
        var tokenEntries = _context.ChangeTracker.Entries<RefreshToken>().ToList();
        tokenEntries.Should().HaveCount(2);

        // Existing token should be Modified (due to Revoke)
        var existingTokenEntry = tokenEntries.First(e => e.Entity.Id == existingToken.Id);
        existingTokenEntry.State.Should().Be(EntityState.Modified, "Existing revoked token should be Modified");

        // New token should be Added (not Modified)
        var newTokenEntry = tokenEntries.First(e => e.Entity.Id != existingToken.Id);
        newTokenEntry.State.Should().Be(EntityState.Added, "New token should be Added, not Modified");

        // Verify save completes without error
        var savedCount = await _repository.SaveChangesAsync();
        savedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateAsyncWithExistingNonRevokedTokensShouldNotMarkAsAdded()
    {
        // Arrange - Create and save a user with multiple tokens
        var email = Email.Create("test2@example.com");
        var (user, _) = User.Create(email, "Test User 2", "hashedPassword123");
        user.AddRefreshToken("token-hash-1", DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");
        user.AddRefreshToken("token-hash-2", DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Clear the change tracker to simulate a new request
        _context.ChangeTracker.Clear();

        // Load the user with tokens
        var loadedUser = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstAsync(u => u.Id == user.Id);

        // Act - Update user without modifying tokens (just trigger UpdateAsync)
        loadedUser.UpdateDisplayName("Updated Name");
        await _repository.UpdateAsync(loadedUser);

        // Assert - Existing non-revoked tokens should remain Unchanged
        var tokenEntries = _context.ChangeTracker.Entries<RefreshToken>().ToList();
        tokenEntries.Should().HaveCount(2);

        foreach (var tokenEntry in tokenEntries)
        {
            tokenEntry.State.Should().Be(EntityState.Unchanged,
                "Existing non-modified tokens should remain Unchanged, not be incorrectly marked as Added");
        }

        // Verify save completes without error
        await _repository.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAsyncRevokeExistingTokenShouldMarkAsModified()
    {
        // Arrange - Create and save a user with a token
        var email = Email.Create("test3@example.com");
        var (user, _) = User.Create(email, "Test User 3", "hashedPassword123");
        user.AddRefreshToken("token-hash", DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Clear the change tracker to simulate a new request
        _context.ChangeTracker.Clear();

        // Load the user with tokens
        var loadedUser = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstAsync(u => u.Id == user.Id);

        // Act - Revoke the token (without adding a new one)
        var existingToken = loadedUser.RefreshTokens.First();
        existingToken.Revoke("logout", null);
        await _repository.UpdateAsync(loadedUser);

        // Assert - Token should be Modified (not Added)
        var tokenEntry = _context.ChangeTracker.Entries<RefreshToken>().Single();
        tokenEntry.State.Should().Be(EntityState.Modified, "Revoked token should be Modified, not Added");

        // Verify save completes without error
        await _repository.SaveChangesAsync();
    }
}
