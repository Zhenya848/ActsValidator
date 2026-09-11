using ChatService.Models.Chats;
using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        
        builder.Property(i => i.Id)
            .HasConversion(i => i.Value, value => MessageId.Create(value));
        
        builder.Property(t => t.Type).IsRequired().HasConversion<string>();
        builder.Property(c => c.Content).IsRequired().HasMaxLength(Constants.MAX_MESSAGE_LENGTH);
        builder.Property(r => r.IsRedacted).IsRequired();
        builder.Property(d => d.IsDeleted).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
    }
}