using PaymentService.Providers.MyTax.Abstractions;
using PaymentService.Providers.MyTax.Entities;

namespace PaymentService.Providers.MyTax.Stores;

public sealed class TaxTokenCache
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _loadedFromStore;
    
    private readonly IServiceScopeFactory? _scopeFactory;

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTimeOffset AccessTokenExpiresAt { get; private set; } = DateTimeOffset.MinValue;
    public DateTimeOffset RefreshTokenExpiresAt { get; private set; } = DateTimeOffset.MinValue;

    public TaxTokenCache(IServiceScopeFactory? scopeFactory = null)
    {
        _scopeFactory = scopeFactory;
    }

    public bool HasValidAccessToken =>
        AccessToken is not null && DateTimeOffset.UtcNow < AccessTokenExpiresAt.AddSeconds(-30);
    public async Task EnsureLoadedFromStoreAsync(CancellationToken ct = default)
    {
        if (_loadedFromStore || _scopeFactory is null) 
            return;

        await _lock.WaitAsync(ct);
        
        try
        {
            if (_loadedFromStore) 
                return;

            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ITaxTokenStore>();

            var stored = await store.LoadAsync(ct);
            
            if (stored is not null)
            {
                AccessToken = stored.AccessToken;
                RefreshToken = stored.RefreshToken;
                AccessTokenExpiresAt = stored.ExpiresAt;
                RefreshTokenExpiresAt = stored.RefreshTokenExpiresAt;
            }

            _loadedFromStore = true;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public async Task<IDisposable> LockAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        return new Releaser(_lock);
    }

    public async Task SetAsync(
        string accessToken,
        string refreshToken,
        DateTimeOffset accessTokenExpiresAt,
        DateTimeOffset refreshTokenExpiresAt,
        CancellationToken ct = default)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpiresAt = accessTokenExpiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        _loadedFromStore = true;

        if (_scopeFactory is null) 
            return;

        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ITaxTokenStore>();

        await store.SaveAsync(new StoredTaxTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        }, ct);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;
        public void Dispose() => _semaphore.Release();
    }
}