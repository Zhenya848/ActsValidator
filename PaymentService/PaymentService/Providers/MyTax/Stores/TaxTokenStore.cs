using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentService.DbContexts;
using PaymentService.Models;
using PaymentService.Options;
using PaymentService.Providers.MyTax.Abstractions;
using PaymentService.Providers.MyTax.Entities;

namespace PaymentService.Providers.MyTax.Stores;

public class TaxTokenStore : ITaxTokenStore
{
    private const string Purpose = "MoyNalog.Tokens.v1";
 
    private readonly IDataProtector _protector;
    private readonly AppDbContext _dbContext;
    private readonly TaxAuthOptions _options;
    private readonly ILogger<TaxTokenStore> _logger;

    public TaxTokenStore(
        IDataProtectionProvider dpProvider, 
        AppDbContext dbContext, 
        IOptions<TaxAuthOptions> options,
        ILogger<TaxTokenStore> logger)
    {
        _protector = dpProvider.CreateProtector(Purpose);
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<StoredTaxTokens?> LoadAsync(CancellationToken ct = default)
    {
        var row = await _dbContext.TaxTokensSessions.SingleOrDefaultAsync(ct);
        
        if (row is null)
            return null;
        
        try
        {
            return new StoredTaxTokens
            {
                AccessToken = _protector.Unprotect(row.AccessToken),
                RefreshToken = _protector.Unprotect(row.RefreshToken),
                ExpiresAt = row.ExpiresAt,
                RefreshTokenExpiresAt = row.RefreshTokenExpiresAt
            };
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            _logger.LogCritical("Failed to decrypt tax tokens");
            
            return null;
        }
    }

    public async Task SaveAsync(StoredTaxTokens tokens, CancellationToken ct = default)
    {
        var row = await _dbContext.TaxTokensSessions.SingleOrDefaultAsync(ct);

        if (row is null)
        {
            var taxTokenSession = TaxTokensSession.Create(
                _protector.Protect(tokens.AccessToken),
                _protector.Protect(tokens.RefreshToken),
                tokens.ExpiresAt,
                tokens.RefreshTokenExpiresAt);

            if (taxTokenSession.IsFailure)
            {
                _logger.LogError("Failed to create tax token session: {Reason}", taxTokenSession.Error.Message);
                
                return;
            }
            
            row = taxTokenSession.Value;
        }

        var updateRowResult = row.Update(
            _protector.Protect(tokens.AccessToken),
            _protector.Protect(tokens.RefreshToken),
            tokens.ExpiresAt,
            tokens.RefreshTokenExpiresAt);

        if (updateRowResult.IsFailure)
        {
            _logger.LogCritical("Failed to update tax token session: {Reason}", updateRowResult.Error.Message);
            
            return;
        }

        if (_dbContext.Entry(row).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            _dbContext.TaxTokensSessions.Add(row);

        await _dbContext.SaveChangesAsync(ct);
    }
}