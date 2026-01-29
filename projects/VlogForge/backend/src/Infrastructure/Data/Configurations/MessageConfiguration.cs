using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Message entity.
/// Story: ACF-012
/// </summary>
public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.ConversationId)
            .HasColumnName("conversation_id")
            .IsRequired();

        // Index for listing messages in a conversation
        builder.HasIndex(m => m.ConversationId);

        // Composite index for unread queries
        builder.HasIndex(m => new { m.ConversationId, m.IsRead });

        // Index for rate limiting queries
        builder.HasIndex(m => new { m.SenderId, m.CreatedAt });

        builder.Property(m => m.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.Property(m => m.Content)
            .HasColumnName("content")
            .HasMaxLength(Message.MaxContentLength)
            .IsRequired();

        builder.Property(m => m.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(m => m.ReadAt)
            .HasColumnName("read_at");

        // FK to Conversation
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

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
