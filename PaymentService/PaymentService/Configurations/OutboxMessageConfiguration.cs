using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Models.Shared.ValueObjects.Id;
using PaymentService.Outbox;

namespace PaymentService.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        
        builder.Property(i => i.Id).HasConversion(
            i => i.Value, 
            value => OutboxMessageId.Create(value));
        builder.HasKey(i => i.Id);
        
        builder.Property(t => t.Type).IsRequired();
        
        builder.Property(p => p.Payload).IsRequired().HasColumnType("jsonb");
        
        builder.Property(o => o.OccurredOn).IsRequired();
        builder.Property(p => p.ProcessedOn).IsRequired(false);
        
        builder.Property(e => e.Error).IsRequired(false);
        
        builder.HasIndex(i => new
        {
            i.OccurredOn,
            i.ProcessedOn
        })
        .HasDatabaseName("idx_outbox_messages_unprocessed")
        .IncludeProperties(i => new
        {
            i.Id,
            i.Type,
            i.Payload
        })
        .HasFilter("processed_on IS NULL");
    }
}