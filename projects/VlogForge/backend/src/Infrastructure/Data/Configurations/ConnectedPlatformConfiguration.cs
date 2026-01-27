using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ConnectedPlatform entity.
/// Story: ACF-002
/// </summary>
public class ConnectedPlatformConfiguration : IEntityTypeConfiguration<ConnectedPlatform>
{
    public void Configure(EntityTypeBuilder<ConnectedPlatform> builder)
    {
        builder.ToTable("connected_platforms");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(cp => cp.ProfileId)
            .HasColumnName("profile_id")
            .IsRequired();

        builder.Property(cp => cp.PlatformType)
            .HasColumnName("platform_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cp => cp.Handle)
            .HasColumnName("handle")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(cp => cp.ProfileUrl)
            .HasColumnName("profile_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cp => cp.IsVerified)
            .HasColumnName("is_verified")
            .HasDefaultValue(false);

        builder.Property(cp => cp.FollowerCount)
            .HasColumnName("follower_count");

        // Unique constraint: one platform type per profile
        builder.HasIndex(cp => new { cp.ProfileId, cp.PlatformType })
            .IsUnique();

        // Audit fields
        builder.Property(cp => cp.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cp => cp.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(cp => cp.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(cp => cp.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(cp => cp.DomainEvents);
    }
}
