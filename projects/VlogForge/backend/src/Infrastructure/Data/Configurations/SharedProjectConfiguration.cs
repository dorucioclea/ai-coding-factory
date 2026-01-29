using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for SharedProject entity.
/// Story: ACF-013
/// </summary>
public class SharedProjectConfiguration : IEntityTypeConfiguration<SharedProject>
{
    public void Configure(EntityTypeBuilder<SharedProject> builder)
    {
        builder.ToTable("shared_projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(SharedProject.MaxNameLength)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(SharedProject.MaxDescriptionLength);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.CollaborationRequestId)
            .HasColumnName("collaboration_request_id")
            .IsRequired();

        builder.HasIndex(p => p.CollaborationRequestId)
            .IsUnique();

        builder.Property(p => p.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.HasIndex(p => p.OwnerId);

        builder.Property(p => p.ClosedAt)
            .HasColumnName("closed_at");

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(p => p.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Concurrency token
        builder.Property(p => p.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);

        // Relationships
        builder.HasMany(p => p.Members)
            .WithOne()
            .HasForeignKey(m => m.SharedProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Tasks)
            .WithOne()
            .HasForeignKey(t => t.SharedProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Links)
            .WithOne()
            .HasForeignKey(l => l.SharedProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Activities)
            .WithOne()
            .HasForeignKey(a => a.SharedProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
