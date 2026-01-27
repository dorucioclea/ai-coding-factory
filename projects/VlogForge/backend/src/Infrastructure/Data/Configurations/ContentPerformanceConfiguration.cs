using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ContentPerformance entity.
/// Story: ACF-004
/// </summary>
public class ContentPerformanceConfiguration : IEntityTypeConfiguration<ContentPerformance>
{
    public void Configure(EntityTypeBuilder<ContentPerformance> builder)
    {
        builder.ToTable("content_performances");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(cp => cp.PlatformConnectionId)
            .HasColumnName("platform_connection_id")
            .IsRequired();

        builder.Property(cp => cp.PlatformType)
            .HasColumnName("platform_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cp => cp.ContentId)
            .HasColumnName("content_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cp => cp.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cp => cp.ThumbnailUrl)
            .HasColumnName("thumbnail_url")
            .HasMaxLength(1000);

        builder.Property(cp => cp.ContentUrl)
            .HasColumnName("content_url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(cp => cp.PublishedAt)
            .HasColumnName("published_at")
            .IsRequired();

        builder.Property(cp => cp.ViewCount)
            .HasColumnName("view_count")
            .IsRequired();

        builder.Property(cp => cp.LikeCount)
            .HasColumnName("like_count")
            .IsRequired();

        builder.Property(cp => cp.CommentCount)
            .HasColumnName("comment_count")
            .IsRequired();

        builder.Property(cp => cp.ShareCount)
            .HasColumnName("share_count")
            .IsRequired();

        builder.Property(cp => cp.EngagementRate)
            .HasColumnName("engagement_rate")
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(cp => cp.LastUpdatedAt)
            .HasColumnName("last_updated_at")
            .IsRequired();

        // Unique constraint: one record per content per connection
        builder.HasIndex(cp => new { cp.PlatformConnectionId, cp.ContentId })
            .IsUnique();

        // Index for sorting by views
        builder.HasIndex(cp => cp.ViewCount);

        // Index for sorting by engagement
        builder.HasIndex(cp => cp.EngagementRate);

        // Index for filtering by platform
        builder.HasIndex(cp => cp.PlatformType);

        // Audit fields
        builder.Property(cp => cp.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cp => cp.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(cp => cp.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(cp => cp.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Foreign key to PlatformConnection
        builder.HasOne<PlatformConnection>()
            .WithMany()
            .HasForeignKey(cp => cp.PlatformConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(cp => cp.DomainEvents);
    }
}
