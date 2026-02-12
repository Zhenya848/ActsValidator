using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain;

namespace UserService.Infrastructure.Configurations;

public class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        builder.ToTable("refresh_sessions");
        
        builder
            .HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(i => i.UserId);

        builder.HasKey(i => i.Id);
        
        builder.Property(j => j.Jti);
        builder.Property(c => c.CreatedAt);
        builder.Property(e => e.ExpiresIn);
    }
}