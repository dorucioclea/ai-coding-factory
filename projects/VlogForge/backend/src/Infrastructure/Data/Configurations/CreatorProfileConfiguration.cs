using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for CreatorProfile entity.
/// Story: ACF-002
/// </summary>
public class CreatorProfileConfiguration : IEntityTypeConfiguration<CreatorProfile>
{
    public void Configure(EntityTypeBuilder<CreatorProfile> builder)
    {
        builder.ToTable("creator_profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.Username)
            .HasColumnName("username")
            .HasMaxLength(CreatorProfile.MaxUsernameLength)
            .IsRequired();

        builder.HasIndex(p => p.Username)
            .IsUnique();

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.Property(p => p.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        // Configure Bio as owned value object
        builder.OwnsOne(p => p.Bio, bio =>
        {
            bio.Property(b => b.Value)
                .HasColumnName("bio")
                .HasMaxLength(Bio.MaxLength);
        });

        builder.Property(p => p.ProfilePictureUrl)
            .HasColumnName("profile_picture_url")
            .HasMaxLength(500);

        builder.Property(p => p.OpenToCollaborations)
            .HasColumnName("open_to_collaborations")
            .HasDefaultValue(false);

        builder.Property(p => p.CollaborationPreferences)
            .HasColumnName("collaboration_preferences")
            .HasMaxLength(500);

        // Configure NicheTags as owned collection
        builder.OwnsMany(p => p.NicheTags, tag =>
        {
            tag.ToTable("creator_profile_niche_tags");

            tag.WithOwner().HasForeignKey("CreatorProfileId");

            tag.Property<int>("Id")
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            tag.HasKey("Id");

            tag.Property(t => t.Value)
                .HasColumnName("tag")
                .HasMaxLength(NicheTag.MaxLength)
                .IsRequired();

            tag.HasIndex("CreatorProfileId", "Value")
                .IsUnique();
        });

        // Configure relationship with ConnectedPlatforms
        builder.HasMany(p => p.ConnectedPlatforms)
            .WithOne()
            .HasForeignKey(cp => cp.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

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

        // Configure navigation to access backing field for NicheTags
        builder.Navigation(p => p.NicheTags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Configure navigation for ConnectedPlatforms
        builder.Navigation(p => p.ConnectedPlatforms)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}
