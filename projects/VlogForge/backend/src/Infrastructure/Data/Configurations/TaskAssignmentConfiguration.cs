using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for TaskAssignment entity.
/// Story: ACF-008
/// </summary>
public class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
{
    public void Configure(EntityTypeBuilder<TaskAssignment> builder)
    {
        builder.ToTable("task_assignments");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.ContentItemId)
            .HasColumnName("content_item_id")
            .IsRequired();

        builder.HasIndex(t => t.ContentItemId);

        // Unique filtered index to prevent duplicate active assignments (race condition protection)
        // Only one non-completed assignment per content item
        builder.HasIndex(t => t.ContentItemId)
            .HasFilter("status <> 2") // 2 = Completed
            .IsUnique()
            .HasDatabaseName("ix_task_assignments_content_item_active_unique");

        builder.Property(t => t.TeamId)
            .HasColumnName("team_id")
            .IsRequired();

        builder.HasIndex(t => t.TeamId);

        builder.Property(t => t.AssigneeId)
            .HasColumnName("assignee_id")
            .IsRequired();

        // Index for efficient "My Tasks" queries sorted by due date
        builder.HasIndex(t => new { t.AssigneeId, t.DueDate });

        builder.Property(t => t.AssignedById)
            .HasColumnName("assigned_by_id")
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        // Index for overdue task queries
        builder.HasIndex(t => new { t.Status, t.DueDate });

        // Index for active assignment check (ContentItemId + Status)
        builder.HasIndex(t => new { t.ContentItemId, t.Status });

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasColumnName("notes")
            .HasMaxLength(TaskAssignment.MaxNotesLength);

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        // Audit fields
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(t => t.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Concurrency token
        builder.Property(t => t.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);

        // Relationships
        builder.HasMany(t => t.Comments)
            .WithOne()
            .HasForeignKey(c => c.TaskAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.History)
            .WithOne()
            .HasForeignKey(h => h.TaskAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
