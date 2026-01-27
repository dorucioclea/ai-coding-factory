using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ContentItem entity.
/// Story: ACF-005
/// </summary>
public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.ToTable("content_items");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(c => c.UserId);

        builder.Property(c => c.Title)
            .HasColumnName("title")
            .HasMaxLength(ContentItem.MaxTitleLength)
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasMaxLength(ContentItem.MaxNotesLength);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(c => c.Status);

        // Configure PlatformTags as JSON column (PostgreSQL jsonb)
        builder.Property(c => c.PlatformTags)
            .HasColumnName("platform_tags")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
                new ValueComparer<IReadOnlyCollection<string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        // Soft delete fields
        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.HasIndex(c => c.IsDeleted);

        builder.Property(c => c.DeletedAt)
            .HasColumnName("deleted_at");

        // Scheduled date (ACF-006)
        builder.Property(c => c.ScheduledDate)
            .HasColumnName("scheduled_date");

        builder.HasIndex(c => c.ScheduledDate);

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Concurrency token
        builder.Property(c => c.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);

        // Global query filter to exclude soft-deleted items by default
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
