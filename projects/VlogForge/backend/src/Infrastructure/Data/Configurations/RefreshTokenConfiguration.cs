using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for RefreshToken entity.
/// Story: ACF-001
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.TokenHash)
            .HasMaxLength(256)
            .IsRequired();

        // Index on TokenHash for fast lookups
        builder.HasIndex(r => r.TokenHash);

        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        builder.Property(r => r.RevokedAt);

        builder.Property(r => r.RevokedBy)
            .HasMaxLength(256);

        builder.Property(r => r.ReplacedByTokenHash)
            .HasMaxLength(256);

        builder.Property(r => r.CreatedByIp)
            .HasMaxLength(45); // Max length for IPv6

        builder.Property(r => r.UserAgent)
            .HasMaxLength(500);

        // Audit fields from Entity base class
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(256);

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(256);

        // Ignore computed properties and domain events
        builder.Ignore(r => r.IsExpired);
        builder.Ignore(r => r.IsRevoked);
        builder.Ignore(r => r.IsActive);
        builder.Ignore(r => r.DomainEvents);
    }
}
