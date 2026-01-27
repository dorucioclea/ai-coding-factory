using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for MetricsSnapshot entity.
/// Story: ACF-004
/// </summary>
public class MetricsSnapshotConfiguration : IEntityTypeConfiguration<MetricsSnapshot>
{
    public void Configure(EntityTypeBuilder<MetricsSnapshot> builder)
    {
        builder.ToTable("metrics_snapshots");

        builder.HasKey(ms => ms.Id);

        builder.Property(ms => ms.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ms => ms.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ms => ms.PlatformType)
            .HasColumnName("platform_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ms => ms.SnapshotDate)
            .HasColumnName("snapshot_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(ms => ms.FollowerCount)
            .HasColumnName("follower_count")
            .IsRequired();

        builder.Property(ms => ms.DailyViews)
            .HasColumnName("daily_views")
            .IsRequired();

        builder.Property(ms => ms.DailyLikes)
            .HasColumnName("daily_likes")
            .IsRequired();

        builder.Property(ms => ms.DailyComments)
            .HasColumnName("daily_comments")
            .IsRequired();

        builder.Property(ms => ms.EngagementRate)
            .HasColumnName("engagement_rate")
            .HasPrecision(10, 4)
            .IsRequired();

        // Unique constraint: one snapshot per user per platform per day
        builder.HasIndex(ms => new { ms.UserId, ms.PlatformType, ms.SnapshotDate })
            .IsUnique();

        // Index for efficient range queries
        builder.HasIndex(ms => new { ms.UserId, ms.SnapshotDate });

        // Index for filtering by platform
        builder.HasIndex(ms => ms.PlatformType);

        // Audit fields
        builder.Property(ms => ms.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ms => ms.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(ms => ms.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(ms => ms.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Foreign key to User
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ms => ms.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(ms => ms.DomainEvents);
    }
}
