using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Models;
using PaymentService.Models.Shared.ValueObjects.Id;

namespace PaymentService.Configurations;

public class PaymentSessionConfiguration : IEntityTypeConfiguration<PaymentSession>
{
    public void Configure(EntityTypeBuilder<PaymentSession> builder)
    {
        builder.ToTable("payment_sessions");
        
        builder.Property(i => i.Id).HasConversion(i => i.Value, value => PaymentSessionId.Create(value));
        builder.HasKey(i => i.Id);
        
        builder.Property(ui => ui.UserId).IsRequired();
        builder.HasOne(p => p.Product).WithMany().HasForeignKey(i => i.ProductId);
    }
}