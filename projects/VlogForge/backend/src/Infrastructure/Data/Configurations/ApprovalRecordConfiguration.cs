using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ApprovalRecord entity.
/// Story: ACF-009
/// </summary>
public class ApprovalRecordConfiguration : IEntityTypeConfiguration<ApprovalRecord>
{
    public void Configure(EntityTypeBuilder<ApprovalRecord> builder)
    {
        builder.ToTable("approval_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.ContentItemId)
            .HasColumnName("content_item_id")
            .IsRequired();

        builder.HasIndex(r => r.ContentItemId);

        builder.Property(r => r.TeamId)
            .HasColumnName("team_id")
            .IsRequired();

        builder.HasIndex(r => r.TeamId);

        builder.Property(r => r.ActorId)
            .HasColumnName("actor_id")
            .IsRequired();

        builder.Property(r => r.Action)
            .HasColumnName("action")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.Feedback)
            .HasColumnName("feedback")
            .HasMaxLength(ApprovalRecord.MaxFeedbackLength);

        builder.Property(r => r.PreviousStatus)
            .HasColumnName("previous_status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.NewStatus)
            .HasColumnName("new_status")
            .HasConversion<int>()
            .IsRequired();

        // Audit fields
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(r => r.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);

        // Composite index for common queries
        builder.HasIndex(r => new { r.ContentItemId, r.CreatedAt });
        builder.HasIndex(r => new { r.TeamId, r.CreatedAt });
    }
}
