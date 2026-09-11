using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.VisualBasic;
using PaymentService.Models;
using PaymentService.Models.Shared.ValueObjects.Id;
using Constants = PaymentService.Models.Shared.Constants;

namespace PaymentService.Configurations;

public class TaxTokensSessionConfiguration : IEntityTypeConfiguration<TaxTokensSession>
{
    public void Configure(EntityTypeBuilder<TaxTokensSession> builder)
    {
        builder.ToTable("tax_token_sessions");
        
        builder.Property(i => i.Id).HasConversion(
            i => i.Value, 
            value => TaxTokensSessionId.Create(value));
        
        builder.Property(a => a.AccessToken).IsRequired().HasMaxLength(Constants.MAX_HIGH_TEXT_LENGTH);
        builder.Property(a => a.RefreshToken).IsRequired().HasMaxLength(Constants.MAX_HIGH_TEXT_LENGTH);
        
        builder.Property(a => a.ExpiresAt).IsRequired();
        builder.Property(a => a.RefreshTokenExpiresAt).IsRequired();
    }
}