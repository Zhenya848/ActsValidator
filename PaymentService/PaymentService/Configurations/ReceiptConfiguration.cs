using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Models;
using PaymentService.Models.Shared.ValueObjects.Id;

namespace PaymentService.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");
        
        builder.Property(i => i.Id).HasConversion(
            i => i.Value, 
            value => ReceiptId.Create(value));

        builder.Property(a => a.Amount).IsRequired();
        builder.Property(c => c.Currency).IsRequired().HasMaxLength(10);
        builder.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(100);
        
        builder.Property(o => o.OccurredOn).IsRequired();
        builder.Property(p => p.ProcessedOn).IsRequired(false);
        
        builder.Property(e => e.Error).IsRequired(false).HasMaxLength(100);
        builder.Property(p => p.PrintUrl).IsRequired(false).HasMaxLength(100);

        builder.HasIndex(i => new
            {
                i.OccurredOn,
                i.ProcessedOn
            })
            .HasDatabaseName("idx_receipts_unprocessed");
    }
}