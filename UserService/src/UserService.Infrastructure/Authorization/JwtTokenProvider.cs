using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions;
using UserService.Application.Models;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Infrastructure.DbContexts;
using UserService.Presentation.Options;

namespace UserService.Infrastructure.Authorization;

public class JwtTokenProvider : ITokenProvider
{
    private readonly AuthOptions _authOptions;
    private readonly AuthDbContext _authDbContext;
    private readonly IKeyProvider _keyProvider;

    public JwtTokenProvider(
        IOptions<AuthOptions> authOptions,
        AuthDbContext authDbContext,
        IKeyProvider keyProvider)
    {
        _authOptions = authOptions.Value;
        _authDbContext = authDbContext;
        _keyProvider = keyProvider;
    }
    
    public JwtTokenResult GenerateAccessToken(User user)
    {
        var rsaKey = _keyProvider.GetPrivateRsa();
        var key = new RsaSecurityKey(rsaKey);
        
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        
        var jti = Guid.NewGuid();
        
        var claims = new[]
        {
            new Claim(CustomClaims.Sub, user.Id.ToString()),
            new Claim(CustomClaims.Jti, jti.ToString()),
            new Claim(CustomClaims.Email, user.Email!),
            new Claim(CustomClaims.Name, user.DisplayName)
        };

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(_authOptions.ExpiredMinutesTime),
            claims: claims,
            signingCredentials: signingCredentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();

        return new JwtTokenResult(tokenHandler.WriteToken(token), jti);
    }

    public async Task<Guid> GenerateRefreshToken(
        User user, 
        Guid accessTokenJti, 
        CancellationToken cancellationToken = default)
    {
        var refreshSession = new RefreshSession
        {
            RefreshToken = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = DateTime.UtcNow.AddDays(_authOptions.ExpiredDaysTimeRefreshToken),
            Jti = accessTokenJti
        };

        _authDbContext.RefreshSessions.Add(refreshSession);
        await _authDbContext.SaveChangesAsync(cancellationToken);

        return refreshSession.RefreshToken;
    }
    
    public async Task<Result<IReadOnlyList<Claim>, ErrorList>> GetUserClaims(string jwtToken)
    {
        var rsaKey = _keyProvider.GetPrivateRsa();
        var key = new RsaSecurityKey(rsaKey);
        
        var jwtHandler = new JwtSecurityTokenHandler();
        
        var validationParameters = TokenValidationParametersFactory.CreateWithoutLifeTime(key);

        var validationResult = await jwtHandler.ValidateTokenAsync(jwtToken, validationParameters);

        if (validationResult.IsValid == false)
            return (ErrorList)Errors.Token.InvalidToken();

        return validationResult.ClaimsIdentity.Claims.ToList();
    }
}