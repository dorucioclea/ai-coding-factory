using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for PlatformMetrics entity.
/// Story: ACF-004
/// </summary>
public class PlatformMetricsConfiguration : IEntityTypeConfiguration<PlatformMetrics>
{
    public void Configure(EntityTypeBuilder<PlatformMetrics> builder)
    {
        builder.ToTable("platform_metrics");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(pm => pm.PlatformConnectionId)
            .HasColumnName("platform_connection_id")
            .IsRequired();

        builder.Property(pm => pm.PlatformType)
            .HasColumnName("platform_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pm => pm.FollowerCount)
            .HasColumnName("follower_count")
            .IsRequired();

        builder.Property(pm => pm.TotalViews)
            .HasColumnName("total_views")
            .IsRequired();

        builder.Property(pm => pm.TotalLikes)
            .HasColumnName("total_likes")
            .IsRequired();

        builder.Property(pm => pm.TotalComments)
            .HasColumnName("total_comments")
            .IsRequired();

        builder.Property(pm => pm.TotalShares)
            .HasColumnName("total_shares")
            .IsRequired();

        builder.Property(pm => pm.EngagementRate)
            .HasColumnName("engagement_rate")
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(pm => pm.MetricsUpdatedAt)
            .HasColumnName("metrics_updated_at")
            .IsRequired();

        // Unique constraint: one metrics record per platform connection
        builder.HasIndex(pm => pm.PlatformConnectionId)
            .IsUnique();

        // Index for quick lookup by platform type
        builder.HasIndex(pm => pm.PlatformType);

        // Audit fields
        builder.Property(pm => pm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pm => pm.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(pm => pm.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(pm => pm.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Foreign key to PlatformConnection
        builder.HasOne<PlatformConnection>()
            .WithMany()
            .HasForeignKey(pm => pm.PlatformConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(pm => pm.DomainEvents);
    }
}
