using ChatService.Models.Chats;
using ChatService.Models.Shared.ValueObjects.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("chats");
        
        builder.Property(i => i.Id)
            .HasConversion(i => i.Value, value => ChatId.Create(value));

        builder.ComplexProperty(u => u.User, ub =>
        {
            ub.Property(i => i.Id).IsRequired().HasColumnName("user_id");
            ub.Property(i => i.Name).IsRequired().HasColumnName("user_name");
            ub.Property(i => i.Email).IsRequired().HasColumnName("user_email");
        });
        
        builder.HasMany(m => m.Messages).WithOne().HasForeignKey(ci => ci.ChatId);
    }
}