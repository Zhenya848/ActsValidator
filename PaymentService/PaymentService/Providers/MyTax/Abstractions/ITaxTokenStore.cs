using PaymentService.Providers.MyTax.Entities;

namespace PaymentService.Providers.MyTax.Abstractions;

public interface ITaxTokenStore
{
    Task<StoredTaxTokens?> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(StoredTaxTokens tokens, CancellationToken ct = default);
}

