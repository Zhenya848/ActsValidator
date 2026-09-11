namespace PaymentService.Providers.MyTax.Entities;

public sealed class StoredTaxTokens
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
}