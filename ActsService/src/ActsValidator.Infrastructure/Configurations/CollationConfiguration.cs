using System.Text.Json;
using ActsValidator.Domain;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
using ActsValidator.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActsValidator.Infrastructure.Configurations;

public class CollationConfiguration : IEntityTypeConfiguration<Collation>
{
    public void Configure(EntityTypeBuilder<Collation> builder)
    {
        builder.ToTable("collations");
        
        builder.Property(i => i.Id)
            .HasConversion(i => i.Value, value => CollationId.Create(value));

        builder.HasOne(a => a.AiRequest).WithOne()
            .HasForeignKey<AiRequest>(ci => ci.CollationId);

        builder.Property(n => n.Act1Name).IsRequired();
        builder.Property(n => n.Act2Name).IsRequired();
        
        builder.Property(d => d.Discrepancies).HasConversion(
                value => JsonSerializer.Serialize(value, JsonSerializerOptions.Default),
                json => JsonSerializer.Deserialize<List<Discrepancy>>(json, JsonSerializerOptions.Default)!)
            .HasColumnType("jsonb")
            .IsRequired(false);
    }
}