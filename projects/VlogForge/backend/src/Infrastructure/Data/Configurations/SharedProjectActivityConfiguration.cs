using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for SharedProjectActivity entity.
/// Story: ACF-013
/// </summary>
public class SharedProjectActivityConfiguration : IEntityTypeConfiguration<SharedProjectActivity>
{
    public void Configure(EntityTypeBuilder<SharedProjectActivity> builder)
    {
        builder.ToTable("shared_project_activities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.SharedProjectId)
            .HasColumnName("shared_project_id")
            .IsRequired();

        builder.HasIndex(a => a.SharedProjectId);

        builder.HasIndex(a => new { a.SharedProjectId, a.CreatedAt });

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.ActivityType)
            .HasColumnName("activity_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.Message)
            .HasColumnName("message")
            .HasMaxLength(SharedProjectActivity.MaxMessageLength)
            .IsRequired();

        // Audit fields
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(a => a.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}
