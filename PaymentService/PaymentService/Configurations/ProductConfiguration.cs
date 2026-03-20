using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Models;
using PaymentService.Models.Shared.ValueObjects.Id;

namespace PaymentService.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(p => p.Price).IsRequired();
        builder.Property(p => p.Pack).IsRequired();
    }
}