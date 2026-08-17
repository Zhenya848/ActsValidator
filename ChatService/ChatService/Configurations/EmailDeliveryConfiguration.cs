using ChatService.Models.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Configurations;

public class EmailDeliveryConfiguration : IEntityTypeConfiguration<EmailDelivery>
{
    public void Configure(EntityTypeBuilder<EmailDelivery> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Recipient)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(5000);

        builder.HasIndex(x => new
            {
                x.OutboxMessageId,
                x.Recipient
            })
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.Status,
            x.NextAttemptAt
        });

        builder.HasOne(o => o.OutboxMessage).WithMany().HasForeignKey(i => i.OutboxMessageId);
    }
}