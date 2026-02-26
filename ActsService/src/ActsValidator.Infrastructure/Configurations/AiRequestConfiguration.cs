using System.Text.Json;
using ActsValidator.Domain;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
using ActsValidator.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActsValidator.Infrastructure.Configurations;

public class AiRequestConfiguration : IEntityTypeConfiguration<AiRequest>
{
    public void Configure(EntityTypeBuilder<AiRequest> builder)
    {
        builder.ToTable("ai_requests");
        
        builder.Property(i => i.Id)
            .HasConversion(i => i.Value, value => AiRequestId.Create(value));
        
        builder.Property(i => i.CollationId)
            .HasConversion(i => i.Value, value => CollationId.Create(value));

        builder.Property(s => s.Status)
            .HasConversion<string>();
        
        builder.Property(d => d.AiDiscrepancies).HasConversion(
                value => JsonSerializer.Serialize(value, JsonSerializerOptions.Default),
                json => JsonSerializer.Deserialize<List<Discrepancy>>(json, JsonSerializerOptions.Default)!)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(e => e.ErrorMessage);
    }
}