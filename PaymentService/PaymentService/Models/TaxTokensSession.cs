using CSharpFunctionalExtensions;
using PaymentService.Models.Shared;
using PaymentService.Models.Shared.ValueObjects.Id;

namespace PaymentService.Models;

public class TaxTokensSession : Shared.Entity<TaxTokensSessionId>
{
    public string AccessToken { get; private set; } = default!;
    public string RefreshToken { get; private set; } = default!;
    
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset RefreshTokenExpiresAt { get; private set; }

    private TaxTokensSession(TaxTokensSessionId id) : base(id)
    {
        
    }
    
    private TaxTokensSession(
        TaxTokensSessionId id, 
        string accessToken, 
        string refreshToken, 
        DateTimeOffset expiresAt, 
        DateTimeOffset refreshTokenExpiresAt) : base(id)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
    }

    public static Result<TaxTokensSession, Error> Create(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        DateTimeOffset refreshTokenExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return Errors.General.ValueIsRequired("access token");
        
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Errors.General.ValueIsRequired("refresh token");
        
        if (expiresAt <= DateTimeOffset.UtcNow)
            return Errors.General.ValueIsInvalid("expires at");
        
        if (refreshTokenExpiresAt <= expiresAt)
            return Errors.General.ValueIsInvalid("refresh token expires at");

        return new TaxTokensSession(
            TaxTokensSessionId.AddNewId(), 
            accessToken, 
            refreshToken, 
            expiresAt,
            refreshTokenExpiresAt);
    }

    public UnitResult<Error> Update(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        DateTimeOffset refreshTokenExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return Errors.General.ValueIsRequired("access token");
        
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Errors.General.ValueIsRequired("refresh token");
        
        if (expiresAt <= DateTimeOffset.UtcNow)
            return Errors.General.ValueIsInvalid("expires at");
        
        if (refreshTokenExpiresAt <= expiresAt)
            return Errors.General.ValueIsInvalid("refresh token expires at");
        
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;

        return Result.Success<Error>();
    }
}