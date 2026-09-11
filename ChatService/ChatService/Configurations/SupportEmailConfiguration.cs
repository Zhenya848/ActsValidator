using ChatService.Models.Email;
using ChatService.Models.Shared.ValueObjects.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Configurations;

public class SupportEmailConfiguration : IEntityTypeConfiguration<SupportEmail>
{
    public void Configure(EntityTypeBuilder<SupportEmail> builder)
    {
        builder.ToTable("support_emails");
        
        builder.Property(i => i.Id)
            .HasConversion(i => i.Value, value => SupportEmailId.Create(value));

        builder.Property(x => x.Email)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.PriorityNumber).IsUnique();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}