using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for SharedProjectMember entity.
/// Story: ACF-013
/// </summary>
public class SharedProjectMemberConfiguration : IEntityTypeConfiguration<SharedProjectMember>
{
    public void Configure(EntityTypeBuilder<SharedProjectMember> builder)
    {
        builder.ToTable("shared_project_members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.SharedProjectId)
            .HasColumnName("shared_project_id")
            .IsRequired();

        builder.Property(m => m.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(m => new { m.SharedProjectId, m.UserId })
            .IsUnique();

        builder.HasIndex(m => m.UserId);

        builder.Property(m => m.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();

        // Audit fields
        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(m => m.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(m => m.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(m => m.DomainEvents);
    }
}
