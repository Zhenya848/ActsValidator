using ActsValidator.Domain;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
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

        builder.OwnsMany(x => x.Act1, a => a.ToJson());
        builder.OwnsMany(x => x.Act2, a => a.ToJson());
        
        builder.OwnsMany(x => x.Discrepancies, d =>
        {
            d.ToJson();
            
            d.OwnsOne(x => x.Act1);
            d.OwnsOne(x => x.Act2);
        });
    }
}