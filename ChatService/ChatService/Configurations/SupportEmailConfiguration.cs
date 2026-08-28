using ChatService.Models.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Configurations;

public class SupportEmailConfiguration : IEntityTypeConfiguration<SupportEmail>
{
    public void Configure(EntityTypeBuilder<SupportEmail> builder)
    {
        builder.HasKey(x => x.Id);

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