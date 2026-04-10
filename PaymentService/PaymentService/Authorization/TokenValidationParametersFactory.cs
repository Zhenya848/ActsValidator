using Microsoft.IdentityModel.Tokens;

namespace PaymentService.Authorization;

public static class TokenValidationParametersFactory
{
    public static TokenValidationParameters CreateWithLifeTime(RsaSecurityKey securityKey)
    {
        return new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero
        };
    }
    
    public static TokenValidationParameters CreateWithoutLifeTime(RsaSecurityKey securityKey)
    {
        return new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero
        };
    }
}