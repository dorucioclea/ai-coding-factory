using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Team entity.
/// Story: ACF-007
/// </summary>
public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.HasIndex(t => t.OwnerId);

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(Team.MaxNameLength)
            .IsRequired();

        builder.HasIndex(t => new { t.OwnerId, t.Name })
            .IsUnique();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(Team.MaxDescriptionLength);

        // Workflow settings (ACF-009)
        builder.Property(t => t.RequiresApproval)
            .HasColumnName("requires_approval")
            .HasDefaultValue(false);

        builder.Property(t => t.ApproverIds)
            .HasColumnName("approver_ids")
            .HasConversion(
                v => string.Join(',', v),
                v => string.IsNullOrEmpty(v)
                    ? new List<Guid>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Guid.Parse)
                        .ToList());

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
        builder.HasMany(t => t.Members)
            .WithOne()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Invitations)
            .WithOne()
            .HasForeignKey(i => i.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
