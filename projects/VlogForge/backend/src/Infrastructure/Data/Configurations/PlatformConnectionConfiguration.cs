using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for PlatformConnection entity.
/// Story: ACF-003
/// </summary>
public class PlatformConnectionConfiguration : IEntityTypeConfiguration<PlatformConnection>
{
    public void Configure(EntityTypeBuilder<PlatformConnection> builder)
    {
        builder.ToTable("platform_connections");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(pc => pc.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(pc => pc.PlatformType)
            .HasColumnName("platform_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pc => pc.EncryptedAccessToken)
            .HasColumnName("encrypted_access_token")
            .HasMaxLength(2000);

        builder.Property(pc => pc.EncryptedRefreshToken)
            .HasColumnName("encrypted_refresh_token")
            .HasMaxLength(2000);

        builder.Property(pc => pc.TokenExpiresAt)
            .HasColumnName("token_expires_at");

        builder.Property(pc => pc.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pc => pc.PlatformAccountId)
            .HasColumnName("platform_account_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(pc => pc.PlatformAccountName)
            .HasColumnName("platform_account_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(pc => pc.LastSyncAt)
            .HasColumnName("last_sync_at");

        builder.Property(pc => pc.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        // Unique constraint: one platform type per user
        builder.HasIndex(pc => new { pc.UserId, pc.PlatformType })
            .IsUnique();

        // Index for finding connections needing refresh
        builder.HasIndex(pc => pc.TokenExpiresAt);

        // Audit fields
        builder.Property(pc => pc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pc => pc.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(pc => pc.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(pc => pc.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Foreign key to User
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(pc => pc.DomainEvents);

        // Ignore computed property
        builder.Ignore(pc => pc.IsTokenExpired);
    }
}
