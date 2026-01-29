using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Conversation entity.
/// Story: ACF-012
/// </summary>
public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Participant1Id)
            .HasColumnName("participant1_id")
            .IsRequired();

        builder.HasIndex(c => c.Participant1Id);

        builder.Property(c => c.Participant2Id)
            .HasColumnName("participant2_id")
            .IsRequired();

        builder.HasIndex(c => c.Participant2Id);

        // Unique index on participant pair
        builder.HasIndex(c => new { c.Participant1Id, c.Participant2Id })
            .IsUnique();

        builder.Property(c => c.LastMessageAt)
            .HasColumnName("last_message_at");

        builder.Property(c => c.LastMessagePreview)
            .HasColumnName("last_message_preview")
            .HasMaxLength(Conversation.MaxPreviewLength + 3); // +3 for "..."

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Concurrency token
        builder.Property(c => c.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}
