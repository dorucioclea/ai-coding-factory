using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for SharedProjectTask entity.
/// Story: ACF-013
/// </summary>
public class SharedProjectTaskConfiguration : IEntityTypeConfiguration<SharedProjectTask>
{
    public void Configure(EntityTypeBuilder<SharedProjectTask> builder)
    {
        builder.ToTable("shared_project_tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.SharedProjectId)
            .HasColumnName("shared_project_id")
            .IsRequired();

        builder.HasIndex(t => t.SharedProjectId);

        builder.Property(t => t.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(SharedProjectTask.MaxTitleLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(SharedProjectTask.MaxDescriptionLength);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.AssigneeId)
            .HasColumnName("assignee_id");

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

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

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);
    }
}
