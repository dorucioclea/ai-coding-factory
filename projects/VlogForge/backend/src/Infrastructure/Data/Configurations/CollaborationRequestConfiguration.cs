using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for CollaborationRequest entity.
/// Story: ACF-011
/// </summary>
public class CollaborationRequestConfiguration : IEntityTypeConfiguration<CollaborationRequest>
{
    public void Configure(EntityTypeBuilder<CollaborationRequest> builder)
    {
        builder.ToTable("collaboration_requests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.HasIndex(r => r.SenderId);

        builder.Property(r => r.RecipientId)
            .HasColumnName("recipient_id")
            .IsRequired();

        builder.HasIndex(r => r.RecipientId);

        // Composite index for checking duplicates
        builder.HasIndex(r => new { r.SenderId, r.RecipientId, r.Status });

        builder.Property(r => r.Message)
            .HasColumnName("message")
            .HasMaxLength(CollaborationRequest.MaxMessageLength)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(r => r.RespondedAt)
            .HasColumnName("responded_at");

        builder.Property(r => r.DeclineReason)
            .HasColumnName("decline_reason")
            .HasMaxLength(CollaborationRequest.MaxDeclineReasonLength);

        // Audit fields
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(r => r.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Concurrency token
        builder.Property(r => r.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
    }
}
