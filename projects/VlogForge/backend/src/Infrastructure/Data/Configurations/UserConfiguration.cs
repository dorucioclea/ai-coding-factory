using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for User entity.
/// Story: ACF-001
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(254)
                .IsRequired();

            email.HasIndex(e => e.Value)
                .IsUnique();
        });

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.EmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailVerificationTokenHash)
            .HasMaxLength(256);

        builder.Property(u => u.EmailVerificationTokenExpiry);

        builder.Property(u => u.PasswordResetTokenHash)
            .HasMaxLength(256);

        builder.Property(u => u.PasswordResetTokenExpiry);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LockoutEnd);

        builder.Property(u => u.LastLoginAt);

        // Audit fields from Entity base class
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(256);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(256);

        // Navigation to RefreshTokens - explicitly configure backing field
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Explicitly specify the backing field name and access mode
        builder.Metadata.FindNavigation(nameof(User.RefreshTokens))!
            .SetField("_refreshTokens");

        builder.Metadata.FindNavigation(nameof(User.RefreshTokens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events collection
        builder.Ignore(u => u.DomainEvents);

        // Version tracking for optimistic concurrency
        builder.Property(u => u.Version)
            .IsConcurrencyToken();
    }
}
