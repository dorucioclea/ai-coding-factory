using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for SharedProjectLink entity.
/// Story: ACF-013
/// </summary>
public class SharedProjectLinkConfiguration : IEntityTypeConfiguration<SharedProjectLink>
{
    public void Configure(EntityTypeBuilder<SharedProjectLink> builder)
    {
        builder.ToTable("shared_project_links");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.SharedProjectId)
            .HasColumnName("shared_project_id")
            .IsRequired();

        builder.HasIndex(l => l.SharedProjectId);

        builder.Property(l => l.AddedByUserId)
            .HasColumnName("added_by_user_id")
            .IsRequired();

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(SharedProjectLink.MaxTitleLength)
            .IsRequired();

        builder.Property(l => l.Url)
            .HasColumnName("url")
            .HasMaxLength(SharedProjectLink.MaxUrlLength)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .HasMaxLength(SharedProjectLink.MaxDescriptionLength);

        // Audit fields
        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(l => l.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(l => l.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(l => l.DomainEvents);
    }
}
