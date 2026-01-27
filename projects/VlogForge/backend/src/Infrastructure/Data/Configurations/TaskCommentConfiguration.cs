using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for TaskComment entity.
/// Story: ACF-008
/// </summary>
public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("task_comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.TaskAssignmentId)
            .HasColumnName("task_assignment_id")
            .IsRequired();

        builder.HasIndex(c => c.TaskAssignmentId);

        builder.Property(c => c.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.HasIndex(c => c.AuthorId);

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .HasMaxLength(TaskComment.MaxContentLength)
            .IsRequired();

        builder.Property(c => c.ParentCommentId)
            .HasColumnName("parent_comment_id");

        // Index for threaded comments
        builder.HasIndex(c => c.ParentCommentId);

        builder.Property(c => c.IsEdited)
            .HasColumnName("is_edited")
            .HasDefaultValue(false);

        builder.Property(c => c.EditedAt)
            .HasColumnName("edited_at");

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

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}
