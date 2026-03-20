using System.Text.Json;
using ActsValidator.Domain;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
using ActsValidator.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActsValidator.Infrastructure.Configurations;

public class CollationConfiguration : IEntityTypeConfiguration<Collation>
{
    public void Configure(EntityTypeBuilder<Collation> builder)
    {
        builder.ToTable("collations");
        
        builder.Property(i => i.Id)
            .HasConversion(i => i.Value, value => CollationId.Create(value));
        
        builder.Property(ui => ui.UserId).IsRequired();

        builder.Property(n => n.Act1Name).IsRequired();
        builder.Property(n => n.Act2Name).IsRequired();
        builder.Property(n => n.CoincidencesCount).IsRequired();
        builder.Property(n => n.RowsProcessed).IsRequired();
        builder.Property(n => n.Status).IsRequired().HasConversion<string>();
        builder.Property(n => n.CreatedAt).IsRequired();
        
        var comparer = new ValueComparer<HashSet<Discrepancy>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => new HashSet<Discrepancy>(c)
        );
        
        builder.Property(d => d.CollationErrors).HasConversion(
                value => JsonSerializer.Serialize(value, JsonSerializerOptions.Default),
                json => JsonSerializer.Deserialize<HashSet<Discrepancy>>(json, JsonSerializerOptions.Default)!,
                comparer)
            .HasColumnType("jsonb")
            .IsRequired(false);
    }
}