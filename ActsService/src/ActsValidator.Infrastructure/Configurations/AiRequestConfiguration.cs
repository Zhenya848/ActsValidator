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
        
        builder.HasOne(x => x.Collation)
            .WithOne()
            .HasForeignKey<AiRequest>("collation_id");

        builder.Property(s => s.Status)
            .HasConversion<string>();

        builder.Property(e => e.ErrorMessage);
    }
}